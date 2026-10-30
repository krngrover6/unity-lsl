using System;
using System.IO;
using UnityEngine;

namespace KaranGrover.Lsl
{
    /// <summary>
    /// Runs once before the first scene loads: applies the network config (lsl_api.cfg in persistentDataPath, else the
    /// LslSettings asset) and, on Android, takes the Wi-Fi multicast lock so LSL can find streams on the network.
    /// </summary>
    public static class LslRuntime
    {
        private static AndroidJavaObject multicastLock;

        /// <summary>True while the Android multicast lock is held.</summary>
        public static bool MulticastLockHeld { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            LslSettings settings = Resources.Load<LslSettings>(LslSettings.ResourceName);
            ApplyConfig(settings);
            if (Application.platform == RuntimePlatform.Android && (settings == null || settings.androidMulticastLock))
                AcquireMulticastLock();
        }

        private static void ApplyConfig(LslSettings settings)
        {
            try
            {
                string file = Path.Combine(Application.persistentDataPath, "lsl_api.cfg");
                if (File.Exists(file))
                {
                    LslLibrary.ApplyConfigFile(file);
                    Debug.Log($"[LSL] config from {file}");
                    return;
                }
                if (settings == null) return;
                string config = settings.BuildConfig();
                if (string.IsNullOrWhiteSpace(config)) return;
                LslLibrary.ApplyConfig(config);
                Debug.Log($"[LSL] config from LslSettings:\n{config}");
            }
            catch (Exception error)
            {
                Debug.LogWarning($"[LSL] config not applied ({error.GetType().Name}: {error.Message})");
            }
        }

        /// <summary>Takes the Android Wi-Fi multicast lock (done automatically at startup). Does nothing elsewhere.</summary>
        public static void AcquireMulticastLock()
        {
            if (Application.platform != RuntimePlatform.Android || MulticastLockHeld) return;
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
                using (AndroidJavaObject wifi = context.Call<AndroidJavaObject>("getSystemService", "wifi"))
                {
                    multicastLock = wifi.Call<AndroidJavaObject>("createMulticastLock", "lsl");
                    multicastLock.Call("setReferenceCounted", false);
                    multicastLock.Call("acquire");
                }
                MulticastLockHeld = true;
                Application.quitting += ReleaseMulticastLock;
                Debug.Log("[LSL] Wi-Fi multicast lock acquired");
            }
            catch (Exception error)
            {
                Debug.LogWarning($"[LSL] no Wi-Fi multicast lock ({error.GetType().Name}: {error.Message}); stream discovery may fail, try KnownPeers");
            }
        }

        /// <summary>Releases the Android Wi-Fi multicast lock (done automatically when the app quits).</summary>
        public static void ReleaseMulticastLock()
        {
            if (multicastLock == null) return;
            try { multicastLock.Call("release"); }
            catch (Exception error) { Debug.LogWarning($"[LSL] releasing the multicast lock: {error.Message}"); }
            multicastLock.Dispose();
            multicastLock = null;
            MulticastLockHeld = false;
        }
    }
}
