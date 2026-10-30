# Changelog

## 0.1.0 (unreleased)
- First version: official liblsl C# wrapper (`LSL.cs`, liblsl-Csharp commit `520c23c18e`), package skeleton,
  licenses.
- liblsl 1.17.7 native libraries: Android arm64-v8a and macOS universal built from source
  (`Tools~/build_native.sh`, see `BUILDING.md`), Windows x64 from the official release.
- Unity layer: `LslSession` (outlets, inlet found by name in the background with optional retry), `LslOutlet`,
  `LslInlet` (non-blocking pulls, clock sync), `LslLibrary` (clock, version, config), `LslSettings` (KnownPeers,
  IPv6, scope, session ID; or `lsl_api.cfg` in persistentDataPath), `LslRuntime` (Android Wi-Fi multicast lock).
- Android: `LslNetwork.androidlib` with INTERNET, ACCESS_NETWORK_STATE, ACCESS_WIFI_STATE, CHANGE_WIFI_MULTICAST_STATE.
- README.
