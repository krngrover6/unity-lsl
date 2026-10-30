# Third-party notices

This package bundles the following open-source parts. Each keeps its own license.

## liblsl-Csharp (`Runtime/ThirdParty/LSL.cs`)
- Source: https://github.com/labstreaminglayer/liblsl-Csharp, commit `520c23c18e` (file `LSL.cs`, unchanged)
- License: MIT, Copyright (c) 2018 Christian Kothe

```
MIT License

Copyright (c) 2018 Christian Kothe

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## liblsl (native libraries in `Runtime/Plugins/`)
- Source: https://github.com/sccn/liblsl, tag `v1.17.7` (commit `64988c6a14b8dc3b3f270ece58eab4f480bfab43`)
- Android arm64-v8a and macOS universal: built from that source for this package (see `BUILDING.md`).
  Windows x64: the official release file `liblsl-1.17.7-Win_amd64.zip`.
- License: MIT, Copyright (C) 2012 Christian A. Kothe

```
Copyright (C) 2012 Christian A. Kothe

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
```

liblsl itself contains these libraries (compiled into the native files):
- **Boost** (bundled subset, `lslboost`) and **Asio**: Boost Software License 1.0, https://www.boost.org/LICENSE_1_0.txt
- **pugixml** 1.15: MIT, Copyright (c) 2006-2025 Arseny Kapoulkine, https://github.com/zeux/pugixml
- **loguru**: public domain, https://github.com/emilk/loguru

## Reference (no code used)
- **StandaloneLSL** (`com.unitylsl.bridge` v1.0.0) by Shad Ahmed, https://github.com/user-shiam/unitylslbridge (MIT).
  It showed which parts a Quest LSL package needs (an arm64 liblsl, network permissions). No code or binaries
  from it are included, and none were decompiled.
