using UnityEngine;

namespace KaranGrover.Lsl
{
    /// <summary>
    /// Optional network settings, applied once at startup before any stream exists.
    /// Create with Assets → Create → LSL → Settings and keep it in a Resources folder named exactly "LslSettings".
    /// A file "lsl_api.cfg" in Application.persistentDataPath wins over this asset (on the Quest it can be changed
    /// with adb without a rebuild).
    /// </summary>
    [CreateAssetMenu(menuName = "LSL/Settings", fileName = "LslSettings")]
    public class LslSettings : ScriptableObject
    {
        public const string ResourceName = "LslSettings";

        [Tooltip("IP addresses or host names to ask directly (unicast), for networks that block multicast (eduroam, many office Wi-Fis). E.g. the lab PC's IP.")]
        public string[] knownPeers = new string[0];

        [Tooltip("allow (default), disable (IPv4 only; try this if the Quest can't find streams), or force.")]
        public string ipv6 = "";

        [Tooltip("machine, link, site (default), organization or global.")]
        public string resolveScope = "";

        [Tooltip("Only streams with the same session ID can see each other (default: \"default\").")]
        public string sessionId = "";

        [Tooltip("Extra lsl_api.cfg text appended as is (advanced).")]
        [TextArea(2, 8)] public string extraConfig = "";

        [Tooltip("Hold an Android Wi-Fi multicast lock while the app runs (needed for stream discovery on the Quest).")]
        public bool androidMulticastLock = true;

        public string BuildConfig()
        {
            string text = LslLibrary.BuildConfig(knownPeers, ipv6, resolveScope, sessionId);
            if (!string.IsNullOrWhiteSpace(extraConfig)) text += extraConfig.Trim() + "\n";
            return text;
        }
    }
}
