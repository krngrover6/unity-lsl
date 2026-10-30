using System;

namespace KaranGrover.Lsl
{
    /// <summary>
    /// A stream this app sends. Pushing is cheap and never blocks; it can be called from any thread.
    /// Timestamps are in <see cref="LslLibrary.LocalClock"/> seconds; 0 = now.
    /// </summary>
    public sealed class LslOutlet : IDisposable
    {
        private readonly LSL.StreamOutlet outlet;
        private readonly string[] marker = new string[1];
        private readonly object markerLock = new object();

        public string Name { get; }
        public int ChannelCount { get; }
        public LslFormat Format { get; }
        public bool IsDisposed { get; private set; }

        internal LslOutlet(string name, string type, int channels, double rate, LslFormat format, string sourceId)
        {
            Name = name;
            ChannelCount = channels;
            Format = format;
            using (var info = new LSL.StreamInfo(name, type, channels, rate, (LSL.channel_format_t)format, sourceId ?? ""))
                outlet = new LSL.StreamOutlet(info);
        }

        /// <summary>True while at least one inlet (e.g. LabRecorder) is connected.</summary>
        public bool HasConsumers => !IsDisposed && outlet.have_consumers();

        /// <summary>Sends one text marker (string outlet with 1 channel).</summary>
        public void PushMarker(string text, double timestamp = 0)
        {
            lock (markerLock)
            {
                marker[0] = text ?? "";
                outlet.push_sample(marker, timestamp);
            }
        }

        /// <summary>Sends one sample (length must equal the channel count).</summary>
        public void PushFloats(float[] sample, double timestamp = 0) => outlet.push_sample(sample, timestamp);

        /// <summary>Sends one sample (length must equal the channel count).</summary>
        public void PushDoubles(double[] sample, double timestamp = 0) => outlet.push_sample(sample, timestamp);

        /// <summary>Sends one sample (length must equal the channel count).</summary>
        public void PushInts(int[] sample, double timestamp = 0) => outlet.push_sample(sample, timestamp);

        /// <summary>Sends one sample (length must equal the channel count).</summary>
        public void PushStrings(string[] sample, double timestamp = 0) => outlet.push_sample(sample, timestamp);

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            outlet.Dispose();
        }
    }
}
