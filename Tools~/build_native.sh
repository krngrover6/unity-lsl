#!/bin/bash
# Builds liblsl for this package from the official source (macOS universal + Android arm64-v8a).
# Usage: Tools~/build_native.sh <liblsl source dir (tag v1.17.7)> <pugixml source dir (tag v1.15)> [build dir]
# Needs: cmake >= 3.23 (e.g. Homebrew), Xcode, and a Unity Android module (NDK + ninja), set UNITY_ANDROID if not default.
set -euo pipefail

LSL_SRC="$(cd "$1" && pwd)"
PUGI_SRC="$(cd "$2" && pwd)"
PKG="$(cd "$(dirname "$0")/.." && pwd)"
BUILD="${3:-$PKG/build}"
UNITY_ANDROID="${UNITY_ANDROID:-/Users/karangrover/Editor/6000.3.25f1/PlaybackEngines/AndroidPlayer}"
NDK="$UNITY_ANDROID/NDK"
NINJA="$UNITY_ANDROID/SDK/cmake/3.22.1/bin/ninja"

COMMON=(
  -G Ninja -DCMAKE_MAKE_PROGRAM="$NINJA"
  -DCMAKE_BUILD_TYPE=Release
  -DLSL_FETCH_PUGIXML=ON -DFETCHCONTENT_SOURCE_DIR_PUGIXML="$PUGI_SRC" -DFETCHCONTENT_FULLY_DISCONNECTED=ON
  -DLSL_UNITTESTS=OFF -DLSL_TOOLS=OFF -DLSL_INSTALL=OFF
)

echo "== macOS universal"
cmake -S "$LSL_SRC" -B "$BUILD/macos" "${COMMON[@]}" \
  -DLSL_FRAMEWORK=OFF -DCMAKE_OSX_ARCHITECTURES="arm64;x86_64" -DCMAKE_OSX_DEPLOYMENT_TARGET=11.0
cmake --build "$BUILD/macos" --target lsl
mkdir -p "$PKG/Runtime/Plugins/macOS"
cp -L "$BUILD/macos/liblsl.dylib" "$PKG/Runtime/Plugins/macOS/liblsl.dylib"
install_name_tool -id @rpath/liblsl.dylib "$PKG/Runtime/Plugins/macOS/liblsl.dylib"
strip -x "$PKG/Runtime/Plugins/macOS/liblsl.dylib"
codesign --force --sign - "$PKG/Runtime/Plugins/macOS/liblsl.dylib"

echo "== Android arm64-v8a (API 29, static libc++, 16 KB pages)"
cmake -S "$LSL_SRC" -B "$BUILD/android-arm64" "${COMMON[@]}" \
  -DCMAKE_TOOLCHAIN_FILE="$NDK/build/cmake/android.toolchain.cmake" \
  -DANDROID_ABI=arm64-v8a -DANDROID_PLATFORM=android-29 -DANDROID_STL=c++_static \
  -DANDROID_SUPPORT_FLEXIBLE_PAGE_SIZES=ON
cmake --build "$BUILD/android-arm64" --target lsl
mkdir -p "$PKG/Runtime/Plugins/Android/arm64-v8a"
"$NDK/toolchains/llvm/prebuilt/darwin-x86_64/bin/llvm-strip" --strip-unneeded \
  -o "$PKG/Runtime/Plugins/Android/arm64-v8a/liblsl.so" "$BUILD/android-arm64/liblsl.so"

echo "== done"
