# TqkLibrary.AudioPlayer.FFmpegAudioReader

Cross-platform FFmpeg audio frame reader.

## Supported runtimes

| OS | Architectures |
|----|---------------|
| Windows | x86, x64, arm64 |
| Linux | x64, arm64 |

The library targets `net462` (Windows only) and `net8.0` (Windows + Linux).
FFmpeg 9.0 native libraries are pulled in automatically through the
`TqkLibrary.FFmpeg.Gpl2.Native.*` packages; the only managed-loaded native module is
the thin wrapper `TqkLibrary.AudioPlayer.FFmpegAudioReader.Native`.

> **License note:** the `Gpl2` FFmpeg build is `GPL-2.0-or-later`. Redistributing an
> application together with these FFmpeg binaries carries the GPL obligations.

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
in the `TqkLibrary.FFmpeg.Gpl2.Native.<RID>` packages.

- **Local (Windows)**: `pwsh src/Build.ps1` builds the Windows RIDs and packs.
- **All RIDs (incl. Linux x64/arm64)**: `.github/workflows/test.yml` builds the
  native wrapper per RID and decodes a sample mp3 on Windows/Linux x64 (every
  push to master and every pull request).

## Release

Version = the tag. Push a tag `vM.N.P` on a commit of `master`:

```
git tag v1.2.0
git push origin v1.2.0
```

`.github/workflows/release.yml` then runs the tests, packs `M.N.P` with every
RID, creates the GitHub Release (notes by git-cliff) and pushes to nuget.org
through NuGet Trusted Publishing. Builds after a tag are `M.N.P-dev.<commits>`.
