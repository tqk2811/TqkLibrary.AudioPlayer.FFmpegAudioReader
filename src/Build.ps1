$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
Set-Location $root

# ---------------------------------------------------------------------------
# FFmpeg native package versions (one NuGet package per RID).
# ---------------------------------------------------------------------------
$ffmpegVersions = @{
    'win-x64'   = '9.0.2'
    'win-x86'   = '9.0.2'
    'win-arm64' = '9.0.2'
}
$nugetCache = Join-Path $env:USERPROFILE '.nuget\packages'

# ---- Locate CMake (prefer the one bundled with Visual Studio) -------------
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$cmake = $null
if (Test-Path $vswhere) {
    $vs = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.CMake.Project -property installationPath | Select-Object -First 1
    $candidate = Join-Path $vs 'Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe'
    if (Test-Path $candidate) { $cmake = $candidate }
}
if (-not $cmake) { $cmake = (Get-Command cmake -ErrorAction SilentlyContinue).Source }
if (-not $cmake) { throw "cmake not found (install Visual Studio C++ CMake tools or cmake on PATH)" }
Write-Host "CMake: $cmake"

# ---- Version via GitVersion ----------------------------------------------
if (-not (Get-Command dotnet-gitversion -ErrorAction SilentlyContinue)) {
    dotnet tool install -g GitVersion.Tool | Out-Host
    $env:PATH = "$env:PATH;$env:USERPROFILE\.dotnet\tools"
}
$gv = dotnet-gitversion /output json | ConvertFrom-Json
$verMajor = [int]$gv.Major; $verMinor = [int]$gv.Minor; $verBuild = [int]$gv.Patch
Write-Host "Version: $($gv.FullSemVer) (native file version $verMajor.$verMinor.$verBuild.0)"

$libProj = "$root\TqkLibrary.AudioPlayer.FFmpegAudioReader\TqkLibrary.AudioPlayer.FFmpegAudioReader.csproj"
$nativeSrc = "$root\TqkLibrary.AudioPlayer.FFmpegAudioReader.Native"
$artifacts = "$root\native-artifacts"

# Restore so the FFmpeg native packages are present in the NuGet cache.
dotnet restore $libProj | Out-Host
if ($LASTEXITCODE -ne 0) { throw "restore failed" }

Remove-Item -Recurse -Force $artifacts -ErrorAction SilentlyContinue

# vsArch by RID. ARM64 is best-effort (requires the ARM64 build tools component).
$archMap = @{ 'win-x64' = 'x64'; 'win-x86' = 'Win32'; 'win-arm64' = 'ARM64' }

function Build-NativeWin([string]$rid) {
    $ver = $ffmpegVersions[$rid]
    $pkg = Join-Path $nugetCache "tqklibrary.ffmpeg.gpl2.native.$($rid.Replace('-','.'))\$ver"
    if (-not (Test-Path $pkg)) { throw "Package not restored: $pkg" }
    $arch = $rid.Substring(4)   # x64 / x86 / arm64
    $vsArch = $archMap[$rid]
    $bld = "$root\native-build\$rid"
    Remove-Item -Recurse -Force $bld -ErrorAction SilentlyContinue

    & $cmake -A $vsArch `
        -DFFMPEG_INCLUDE_DIR="$pkg\build\native\include" `
        -DFFMPEG_LIB_DIR="$pkg\build\native\win\$arch\lib" `
        -DVER_FILE_MAJOR=$verMajor -DVER_FILE_MINOR=$verMinor -DVER_FILE_BUILD=$verBuild `
        -S $nativeSrc -B $bld
    if ($LASTEXITCODE -ne 0) { throw "cmake configure failed ($rid)" }
    & $cmake --build $bld --config Release
    if ($LASTEXITCODE -ne 0) { throw "cmake build failed ($rid)" }

    $dstDir = "$artifacts\runtimes\$rid\native"
    New-Item -ItemType Directory -Force $dstDir | Out-Null
    Get-ChildItem -Recurse $bld -Include '*.dll', '*.pdb' |
        Where-Object { $_.Name -like 'TqkLibrary.AudioPlayer.FFmpegAudioReader.Native.*' } |
        ForEach-Object { Copy-Item $_.FullName $dstDir -Force }
    Write-Host "Native built -> $dstDir"
}

# x64 + x86 are always built locally; arm64 is best-effort.
Build-NativeWin 'win-x64'
Build-NativeWin 'win-x86'
try { Build-NativeWin 'win-arm64' }
catch { Write-Warning "win-arm64 native skipped: $($_.Exception.Message). Linux + full matrix are produced by CI." }

# ---- Pack the managed library --------------------------------------------
Remove-Item -Recurse -Force "$root\TqkLibrary.AudioPlayer.FFmpegAudioReader\bin\Release" -ErrorAction SilentlyContinue
dotnet pack $libProj -c Release -o "$root\TqkLibrary.AudioPlayer.FFmpegAudioReader\bin\Release"
if ($LASTEXITCODE -ne 0) { throw "dotnet pack failed" }
$nupkg = Get-ChildItem "$root\TqkLibrary.AudioPlayer.FFmpegAudioReader\bin\Release\*.nupkg" | Select-Object -First 1
Write-Host "Packed: $($nupkg.Name)"
Write-Warning "This local package only contains the RIDs built on this machine. The published cross-platform package (incl. Linux x64/arm64) is produced by the GitHub Actions workflow."

if (![string]::IsNullOrWhiteSpace($env:localNuget)) { Copy-Item $nupkg.FullName -Destination $env:localNuget -Force }
