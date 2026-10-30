using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KaranGrover.Lsl
{
    /// <summary>
    /// Owns the streams one script makes (outlets and inlets) and closes them all on Dispose.
    /// Finding a stream (resolve) runs on a background thread; awaiting it from Unity code continues on the main thread.
    /// </summary>
    public sealed class LslSession : IDisposable
    {
        private readonly List<IDisposable> streams = new List<IDisposable>();
        private volatile bool disposed;

        public bool IsDisposed => disposed;

        /// <summary>A text marker outlet: 1 string channel, irregular rate.</summary>
        public LslOutlet CreateMarkerOutlet(string name, string type = "Markers", string sourceId = null)
            => CreateOutlet(name, type, 1, 0, LslFormat.String, sourceId);

        /// <summary>A data outlet. rate = nominal samples per second (0 = irregular).</summary>
        public LslOutlet CreateOutlet(string name, string type, int channels, double rate, LslFormat format, string sourceId = null)
        {
            ThrowIfDisposed();
            var outlet = new LslOutlet(name, type, channels, rate, format, sourceId ?? DefaultSourceId(name));
            Track(outlet);
            return outlet;
        }

        /// <summary>
        /// One try to find a stream where prop ("name", "type", "source_id", ...) equals value, waiting up to
        /// timeoutSeconds. Returns a ready inlet, or null if none was found (or the session was disposed meanwhile).
        /// </summary>
        public async Task<LslInlet> ConnectAsync(string prop, string value, double timeoutSeconds = 5,
            bool clockSync = true, int maxBufferSeconds = 360)
        {
            ThrowIfDisposed();
            LslInlet inlet = await Task.Run(() => Resolve(prop, value, timeoutSeconds, clockSync, maxBufferSeconds));
            if (inlet == null) return null;
            if (disposed)
            {
                inlet.Dispose();
                return null;
            }
            Track(inlet);
            return inlet;
        }

        /// <summary>
        /// Keeps trying <see cref="ConnectAsync"/> every retrySeconds until the stream is found, the token is cancelled
        /// or the session is disposed (then returns null). onRetry is called (main thread) after each miss.
        /// </summary>
        public async Task<LslInlet> ConnectWithRetryAsync(string prop, string value, double timeoutSeconds = 2,
            double retrySeconds = 5, CancellationToken cancel = default, Action<int> onRetry = null,
            bool clockSync = true, int maxBufferSeconds = 360)
        {
            for (int attempt = 1; !disposed && !cancel.IsCancellationRequested; attempt++)
            {
                LslInlet inlet = await ConnectAsync(prop, value, timeoutSeconds, clockSync, maxBufferSeconds);
                if (inlet != null) return inlet;
                onRetry?.Invoke(attempt);
                try { await Task.Delay(TimeSpan.FromSeconds(retrySeconds), cancel); }
                catch (TaskCanceledException) { break; }
            }
            return null;
        }

        /// <summary>Closes one stream made by this session early.</summary>
        public void Close(IDisposable stream)
        {
            if (stream == null) return;
            lock (streams) streams.Remove(stream);
            stream.Dispose();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            IDisposable[] all;
            lock (streams)
            {
                all = streams.ToArray();
                streams.Clear();
            }
            foreach (IDisposable stream in all) stream.Dispose();
        }

        private static LslInlet Resolve(string prop, string value, double timeout, bool clockSync, int maxBufferSeconds)
        {
            LSL.StreamInfo[] found = LSL.LSL.resolve_stream(prop, value, 1, timeout);
            try
            {
                if (found.Length == 0) return null;
                var inlet = new LslInlet(found[0], clockSync, maxBufferSeconds);
                try { inlet.Open(timeout); }
                catch
                {
                    inlet.Dispose();
                    throw;
                }
                return inlet;
            }
            finally { LSL.LSL.DisposeArray(found); }
        }

        private static string DefaultSourceId(string name)
        {
            string app = UnityEngine.Application.productName;
            return string.IsNullOrEmpty(app) ? name : app + "." + name;
        }

        private void Track(IDisposable stream)
        {
            lock (streams) streams.Add(stream);
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(LslSession));
        }
    }
}
