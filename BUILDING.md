# Native libraries: where they come from

All three are **liblsl 1.17.7** (tag `v1.17.7`, commit `64988c6a14b8dc3b3f270ece58eab4f480bfab43`, https://github.com/sccn/liblsl).

| File | Platform | How |
|---|---|---|
| `Runtime/Plugins/Android/arm64-v8a/liblsl.so` | Android arm64 (Meta Quest) | built from source, NDK r27c (27.2.12479018, from Unity 6000.3.25f1), API 29, static libc++, 16 KB page alignment, stripped |
| `Runtime/Plugins/macOS/liblsl.dylib` | macOS universal (arm64 + x86_64), editor + standalone | built from source, Xcode (AppleClang 17), macOS 11.0+, install name `@rpath/liblsl.dylib`, ad-hoc signed |
| `Runtime/Plugins/Windows/x86_64/lsl.dll` | Windows x64, editor + standalone | official release file `liblsl-1.17.7-Win_amd64.zip` (`bin/lsl.dll`), unchanged |

pugixml (a liblsl dependency) is **v1.15** (commit `ee86beb30e4973f5feffe3ce63bfa4fbadf72f38`, https://github.com/zeux/pugixml),
compiled statically into liblsl.

## Rebuild (Mac)
Needs cmake >= 3.23 (Homebrew), Xcode, and Unity's Android module (NDK + ninja).

```bash
git clone --depth 1 --branch v1.17.7 https://github.com/sccn/liblsl.git /tmp/liblsl
git clone --depth 1 --branch v1.15 https://github.com/zeux/pugixml.git /tmp/pugixml
Tools~/build_native.sh /tmp/liblsl /tmp/pugixml
```

Set `UNITY_ANDROID=<Unity>/PlaybackEngines/AndroidPlayer` if Unity is not at the default path in the script.
