#ifndef _H_AudioSource_H_
#define _H_AudioSource_H_
class AudioSource
{
public:
	AudioSource();
	~AudioSource();

	BOOL Init(LPSTR filePath);

	BOOL ReadFrame(AVFrame* pframe);

private:
	AVFormatContext* pInputFormatContext{ nullptr };
	AVCodecContext* pCodecContext{ nullptr };
	int audioIndex = -1;
	AVStream* input_audio_stream{ nullptr };
};
#endif