# TqkLibrary.AudioPlayer.FFmpegAudioReader

Cross-platform FFmpeg audio frame reader.

## Supported runtimes

| OS | Architectures |
|----|---------------|
| Windows | x86, x64, arm64 |
| Linux | x64, arm64 |

The library targets `net462` (Windows only) and `net8.0` (Windows + Linux).
FFmpeg 8.0 native libraries are pulled in automatically through the
`TqkLibrary.FFmpeg.Native.*` packages; the only managed-loaded native module is
the thin wrapper `TqkLibrary.AudioPlayer.FFmpegAudioReader.Native`.

## Usage

```csharp
using TqkLibrary.AudioPlayer.FFmpegAudioReader;

using var audioSource = new AudioSource("audio.mp3");
using var frame = new AudioSource.AVFrame();
while (audioSource.ReadFrame(frame))
{
    // process frame.Handle (AVFrame*)
}
```

## Building

The native wrapper is built with CMake against the headers/import-libs shipped
in the `TqkLibrary.FFmpeg.Native.<RID>` packages.

- **Local (Windows)**: `pwsh src/Build.ps1` builds the Windows RIDs and packs.
- **All RIDs (incl. Linux x64/arm64)**: produced by the GitHub Actions
  `build` workflow (`.github/workflows/build.yml`), which builds the native
  wrapper per RID, smoke-tests it on Windows/Linux x64, then packs.
