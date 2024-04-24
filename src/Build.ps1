Remove-Item -Recurse -Force .\x64\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\x86\Release\** -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force .\TqkLibrary.AudioPlayer.FFmpegAudioReader\bin\Release\** -ErrorAction SilentlyContinue

$env:PATH="$($env:PATH);C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE"
devenv .\TqkLibrary.AudioPlayer.FFmpegAudioReader.sln /Rebuild 'Release|x64' /Project TqkLibrary.AudioPlayer.FFmpegAudioReader.Native
devenv .\TqkLibrary.AudioPlayer.FFmpegAudioReader.sln /Rebuild 'Release|x86' /Project TqkLibrary.AudioPlayer.FFmpegAudioReader.Native

dotnet build --no-incremental .\TqkLibrary.AudioPlayer.FFmpegAudioReader\TqkLibrary.AudioPlayer.FFmpegAudioReader.csproj -c Release
nuget pack .\TqkLibrary.AudioPlayer.FFmpegAudioReader\TqkLibrary.AudioPlayer.FFmpegAudioReader.nuspec -Symbols -OutputDirectory .\TqkLibrary.AudioPlayer.FFmpegAudioReader\bin\Release

$localNuget = $env:localNuget
if(![string]::IsNullOrWhiteSpace($localNuget))
{
    Copy-Item .\TqkLibrary.AudioPlayer.FFmpegAudioReader\bin\Release\*.nupkg -Destination $localNuget -Force
}

$nugetKey = $env:nugetKey
if(![string]::IsNullOrWhiteSpace($nugetKey))
{
    Write-Host "Enter to push nuget"
    pause
    Write-Host "enter to confirm"
    pause
    $files = [System.IO.Directory]::GetFiles(".\TqkLibrary.AudioPlayer.FFmpegAudioReader\bin\Release\")
    iex "nuget push $($files[0]) -ApiKey $($nugetKey) -Source https://api.nuget.org/v3/index.json"
}