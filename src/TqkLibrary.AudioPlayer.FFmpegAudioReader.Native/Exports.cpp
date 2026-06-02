#include "pch.h"
#include "AudioSource.h"
#include "Exports.h"

AudioSource* AudioSource_Alloc(const char* filePath) {
	g_lastAvError = 0;
	AudioSource* pAudioSource = new AudioSource();
	if (pAudioSource->Init(filePath))
	{
		return pAudioSource;
	}
	else
	{
		delete pAudioSource;
		return nullptr;
	}
}
void AudioSource_Free(AudioSource** ppAudioSource) {
	if (ppAudioSource)
	{
		AudioSource* pAudioSource = *ppAudioSource;
		if (pAudioSource)
		{
			delete pAudioSource;
			*ppAudioSource = nullptr;
		}
	}
}
int AudioSource_ReadFrame(AudioSource* pAudioSource, AVFrame* pframe) {
	g_lastAvError = 0;
	if (!pAudioSource)
		return 0;
	return pAudioSource->ReadFrame(pframe) ? 1 : 0;
}
AVFrame* AudioSource_FrameAlloc() {
	return av_frame_alloc();
}
void AudioSource_FrameFree(AVFrame** ppFrame) {
	if (ppFrame && *ppFrame)
		av_frame_free(ppFrame);
}
int AudioSource_GetLastError() {
	return g_lastAvError;
}
