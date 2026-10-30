# LSL for Unity (`com.karangrover.lsl`)

[Lab Streaming Layer](https://labstreaminglayer.org) for Unity, made for **Meta Quest (standalone Android)** and also
working in the **Mac and Windows editor / standalone**. It has:

- the official C# wrapper (`LSL.cs` from [liblsl-Csharp](https://github.com/labstreaminglayer/liblsl-Csharp), unchanged);
- **liblsl 1.17.7** for Android arm64-v8a, macOS universal and Windows x64 (see [BUILDING.md](BUILDING.md));
- a small Unity layer: outlets, an inlet found by name in the background (with retry), a pull that never blocks
  (safe every frame), the LSL clock, time correction;
- on Android: the network permissions and a **Wi-Fi multicast lock** taken automatically at startup, so stream
  discovery works on the Quest.

## Install
**Package Manager → + → Install package from git URL:**
```
https://github.com/krngrover6/unity-lsl.git#v0.1.0
```
or **Install package from disk…** and pick this folder's `package.json` (local path).

Only one liblsl may be in a project: remove other LSL packages / `liblsl` plugins first.

## Use
```csharp
using KaranGrover.Lsl;

LslSession lsl = new LslSession();                     // owns the streams; lsl.Dispose() in OnDestroy
LslOutlet markers = lsl.CreateMarkerOutlet("MyMarkers");
LslOutlet motion  = lsl.CreateOutlet("MyMotion", "Motion", 23, 90, LslFormat.Float32);

markers.PushMarker("trial_start", LslLibrary.LocalClock);   // explicit timestamp (or 0 = now)
motion.PushFloats(sample23);                                // float[23]

// Find a stream by name in the background (main thread continues when found). null = not found in time.
LslInlet inlet = await lsl.ConnectAsync("name", "MyAttention", 2);
// or keep trying every 5 s:
LslInlet inlet = await lsl.ConnectWithRetryAsync("name", "MyAttention", 2, 5, cancelToken);

// Every frame (never blocks):
var buffer = new double[inlet.ChannelCount];
while (inlet.TryPullDoubles(buffer, out double time)) { /* time is already in LslLibrary.LocalClock */ }
```

| Type | Main members |
|---|---|
| `LslSession` | `CreateMarkerOutlet`, `CreateOutlet`, `ConnectAsync`, `ConnectWithRetryAsync`, `Close`, `Dispose` |
| `LslOutlet` | `PushMarker`, `PushFloats`, `PushDoubles`, `PushInts`, `PushStrings`, `HasConsumers` |
| `LslInlet` | `TryPullDoubles`, `TryPullFloats`, `TryPullMarker`, `Available`, `TimeCorrection`, `UpdateTimeCorrection`, stream info (`Name`, `ChannelCount`, ...) |
| `LslLibrary` | `LocalClock`, `Version`, `Info`, `ApplyConfig`, `ApplyConfigFile`, `BuildConfig` |
| `LslRuntime` | `MulticastLockHeld`, `AcquireMulticastLock`, `ReleaseMulticastLock` |

- Inlets use **clock sync** by default: pulled timestamps are already mapped to this machine's clock
  (`clockSync: false` to get raw remote times; then add `inlet.TimeCorrection`).
- A `TryPull` throws `LSL.LostException` if the source is gone for good; make a new inlet then.
- Native LSL errors throw normal C# exceptions; wrap the first `new LslSession()` / `CreateOutlet` in try/catch if the
  app must run where LSL can't start.

## Quest / network notes
- The Quest and the PC must be on the **same network** (same Wi-Fi, or the PC wired to the same router).
  **eduroam and many university/office Wi-Fis block multicast**, so streams are not found. Best: a small dedicated
  router for the lab.
- Discovery uses UDP multicast. The package holds an Android **multicast lock** (log: `[LSL] Wi-Fi multicast lock acquired`).
- If streams are still not found, use **KnownPeers** (direct IPs, no multicast needed):
  **Assets → Create → LSL → Settings**, save it in a `Resources` folder as **`LslSettings`**, and add the other
  machine's IP (e.g. the lab PC) to **Known Peers**. The PC side needs the Quest's IP in its own `lsl_api.cfg`
  (`[lab]` `KnownPeers = {192.168.1.30}`) for the other direction.
- If the Quest still can't see streams, try **IPv6 = disable** in the same settings.
- Without a rebuild: put an `lsl_api.cfg` into the app's data folder; it wins over the asset:
  `adb push lsl_api.cfg /sdcard/Android/data/<package name>/files/lsl_api.cfg`
- liblsl reads its config **once per process**: in the editor, restart Unity after changing settings.
- Unity's **Player → Android → Internet Access** can stay on Auto; the package's manifest adds the permissions.

## Credits
- liblsl and liblsl-Csharp by Christian Kothe and the LSL contributors (MIT). See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
- **StandaloneLSL** (`com.unitylsl.bridge`) by Shad Ahmed, https://github.com/user-shiam/unitylslbridge (MIT), was the
  reference for which parts a Quest LSL package needs. No code or binaries from it are used.

## License
MIT (see [LICENSE](LICENSE)). Bundled parts keep their own licenses (MIT / Boost 1.0 / public domain).
