using TqkLibrary.AudioPlayer.FFmpegAudioReader;

// Resolve the sample next to the executable so the test works from any working directory and on Linux.
string filePath = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "test-tube-194556.mp3");

int frameCount = 0;
using AudioSource audioSource = new AudioSource(filePath);
using AudioSource.AVFrame frame = new AudioSource.AVFrame();
while (audioSource.ReadFrame(frame))
{
    frameCount++;
}
Console.WriteLine($"Decoded {frameCount} frames");
if (frameCount == 0)
    throw new InvalidOperationException("No audio frame decoded");
Console.WriteLine("Exit");
