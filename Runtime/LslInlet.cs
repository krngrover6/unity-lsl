using System;

namespace KaranGrover.Lsl
{
    /// <summary>
    /// A stream this app receives. Made by <see cref="LslSession.ConnectAsync"/>, which opens the stream and fetches the
    /// first time correction in the background, so the TryPull calls never block (safe to call every frame).
    /// With clock sync on (default), timestamps are already in this machine's <see cref="LslLibrary.LocalClock"/>.
    /// A TryPull throws LSL.LostException if the source is gone for good.
    /// </summary>
    public sealed class LslInlet : IDisposable
    {
        private readonly LSL.StreamInlet inlet;
        private readonly string[] text = new string[1];

        public string Name { get; }
        public string Type { get; }
        public string SourceId { get; }
        public string Hostname { get; }
        public int ChannelCount { get; }
        public double NominalRate { get; }
        public LslFormat Format { get; }
        public bool ClockSync { get; }
        public bool IsDisposed { get; private set; }

        /// <summary>Last known offset (seconds) to add to a remote timestamp to get local time (already applied with ClockSync).</summary>
        public double TimeCorrection { get; private set; }

        internal LslInlet(LSL.StreamInfo info, bool clockSync, int maxBufferSeconds)
        {
            Name = info.name();
            Type = info.type();
            SourceId = info.source_id();
            Hostname = info.hostname();
            ChannelCount = info.channel_count();
            NominalRate = info.nominal_srate();
            Format = (LslFormat)info.channel_format();
            ClockSync = clockSync;
            inlet = new LSL.StreamInlet(info, maxBufferSeconds, 0, true,
                clockSync ? LSL.processing_options_t.proc_clocksync : LSL.processing_options_t.proc_none);
        }

        /// <summary>Blocking setup, run off the main thread by LslSession: open the stream and get the first time correction.</summary>
        internal void Open(double timeout)
        {
            inlet.open_stream(timeout);
            TimeCorrection = inlet.time_correction(timeout);
        }

        /// <summary>Number of samples waiting (may be an under-estimate).</summary>
        public int Available => inlet.samples_available();

        /// <summary>Takes the next waiting sample, if any. buffer length must equal the channel count.</summary>
        public bool TryPullDoubles(double[] buffer, out double timestamp)
        {
            timestamp = inlet.pull_sample(buffer, 0.0);
            return timestamp != 0.0;
        }

        /// <summary>Takes the next waiting sample, if any. buffer length must equal the channel count.</summary>
        public bool TryPullFloats(float[] buffer, out double timestamp)
        {
            timestamp = inlet.pull_sample(buffer, 0.0);
            return timestamp != 0.0;
        }

        /// <summary>Takes the next waiting text marker, if any (string stream with 1 channel).</summary>
        public bool TryPullMarker(out string marker, out double timestamp)
        {
            timestamp = inlet.pull_sample(text, 0.0);
            marker = timestamp != 0.0 ? text[0] : null;
            return timestamp != 0.0;
        }

        /// <summary>Refreshes <see cref="TimeCorrection"/> without waiting (liblsl updates it in the background).</summary>
        public double UpdateTimeCorrection()
        {
            try { TimeCorrection = inlet.time_correction(0.0); }
            catch (TimeoutException) { }
            return TimeCorrection;
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            inlet.Dispose();
        }
    }
}
