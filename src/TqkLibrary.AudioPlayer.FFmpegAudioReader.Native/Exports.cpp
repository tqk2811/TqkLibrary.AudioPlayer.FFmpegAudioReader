#include "pch.h"
#include "Exports.h"

AudioSource* AudioSource_Alloc(LPSTR filePath) {
	SetLastError(0);
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
VOID AudioSource_Free(AudioSource** ppAudioSource) {
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
BOOL AudioSource_ReadFrame(AudioSource* pAudioSource, AVFrame* pframe) {
	SetLastError(0);
	if (!pAudioSource)
		return FALSE;
	return pAudioSource->ReadFrame(pframe);
}