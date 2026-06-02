#ifndef _H_AudioSource_H_
#define _H_AudioSource_H_
#include "libav.h"
class AudioSource
{
public:
	AudioSource();
	~AudioSource();

	bool Init(const char* filePath);

	bool ReadFrame(AVFrame* pframe);

private:
	AVFormatContext* pInputFormatContext{ nullptr };
	AVCodecContext* pCodecContext{ nullptr };
	int audioIndex = -1;
	AVStream* input_audio_stream{ nullptr };
};
#endif
