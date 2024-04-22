using TqkLibrary.AudioPlayer.FFmpegAudioReader;

using AudioSource audioSource = new AudioSource(".\\test-tube-194556.mp3");
using AudioSource.AVFrame frame = new AudioSource.AVFrame();
while (audioSource.ReadFrame(frame))
{

}
Console.WriteLine("Exit");