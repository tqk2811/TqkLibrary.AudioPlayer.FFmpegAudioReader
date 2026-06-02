#ifndef _H_TqkLibraryAudioPlayerFFmpegAudioReaderNative_H_
#define _H_TqkLibraryAudioPlayerFFmpegAudioReaderNative_H_

#include "platform.h"
#include "libav.h"

class AudioSource;

DLL_EXPORT AudioSource* AudioSource_Alloc(const char* filePath);
DLL_EXPORT void AudioSource_Free(AudioSource** ppAudioSource);
DLL_EXPORT int AudioSource_ReadFrame(AudioSource* pAudioSource, AVFrame* pframe);

// Frame helpers are exported here so managed code only ever P/Invokes this one
// native library; the FFmpeg shared libs are loaded transitively as its
// dependencies (kept platform-name agnostic on the managed side).
DLL_EXPORT AVFrame* AudioSource_FrameAlloc();
DLL_EXPORT void AudioSource_FrameFree(AVFrame** ppFrame);

// Returns the last FFmpeg error code captured on the current thread.
DLL_EXPORT int AudioSource_GetLastError();

#endif // !_H_TqkLibraryAudioPlayerFFmpegAudioReaderNative_H_
