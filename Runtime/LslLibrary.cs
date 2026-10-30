using System;
using System.Runtime.InteropServices;

namespace KaranGrover.Lsl
{
    /// <summary>
    /// Library-wide LSL helpers: the LSL clock, the version, and the network config (KnownPeers etc.).
    /// </summary>
    public static class LslLibrary
    {
        /// <summary>The LSL clock in seconds (the time base of all LSL timestamps on this machine).</summary>
        public static double LocalClock => LSL.LSL.local_clock();

        /// <summary>liblsl version as a number (e.g. 117 = 1.17).</summary>
        public static int Version => LSL.LSL.library_version();

        /// <summary>liblsl build info (git tag, compiler, ...).</summary>
        public static string Info => Marshal.PtrToStringAnsi(Native.lsl_library_info());

        /// <summary>The config that was applied with <see cref="ApplyConfig"/> or <see cref="ApplyConfigFile"/>, or null.</summary>
        public static string AppliedConfig { get; private set; }

        /// <summary>
        /// Sets liblsl's config (lsl_api.cfg format, e.g. "[lab]\nKnownPeers = {192.168.1.20}").
        /// Only works before the first stream is created or resolved; liblsl reads its config once per process.
        /// </summary>
        public static void ApplyConfig(string content)
        {
            Native.lsl_set_config_content(content ?? "");
            AppliedConfig = content;
        }

        /// <summary>Same as <see cref="ApplyConfig"/>, but from an lsl_api.cfg file.</summary>
        public static void ApplyConfigFile(string path)
        {
            Native.lsl_set_config_filename(path);
            AppliedConfig = "file: " + path;
        }

        /// <summary>Builds an lsl_api.cfg text from the common options (empty / null values are left out).</summary>
        public static string BuildConfig(string[] knownPeers, string ipv6 = null, string resolveScope = null, string sessionId = null)
        {
            var text = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(ipv6)) text.Append("[ports]\nIPv6 = ").Append(ipv6).Append('\n');
            if (!string.IsNullOrEmpty(resolveScope)) text.Append("[multicast]\nResolveScope = ").Append(resolveScope).Append('\n');
            bool hasPeers = knownPeers != null && Array.Exists(knownPeers, peer => !string.IsNullOrWhiteSpace(peer));
            if (hasPeers || !string.IsNullOrEmpty(sessionId))
            {
                text.Append("[lab]\n");
                if (hasPeers) text.Append("KnownPeers = {").Append(string.Join(", ", Array.FindAll(knownPeers, peer => !string.IsNullOrWhiteSpace(peer)))).Append("}\n");
                if (!string.IsNullOrEmpty(sessionId)) text.Append("SessionID = ").Append(sessionId).Append('\n');
            }
            return text.ToString();
        }

        private static class Native
        {
            private const string Lib = "lsl";

            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
            public static extern void lsl_set_config_content(string content);

            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
            public static extern void lsl_set_config_filename(string filename);

            [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
            public static extern IntPtr lsl_library_info();
        }
    }
}
