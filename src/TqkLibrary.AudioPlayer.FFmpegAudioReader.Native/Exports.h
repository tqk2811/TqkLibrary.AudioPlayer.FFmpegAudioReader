#ifndef _H_TqkLibraryAudioPlayerFFmpegAudioReaderNative_H_
#define _H_TqkLibraryAudioPlayerFFmpegAudioReaderNative_H_

TQKLIBRARYAUDIOPLAYERFFMPEGAUDIOREADERNATIVE__EXPORTS AudioSource* AudioSource_Alloc(LPSTR filePath);
TQKLIBRARYAUDIOPLAYERFFMPEGAUDIOREADERNATIVE__EXPORTS VOID AudioSource_Free(AudioSource** ppAudioSource);
TQKLIBRARYAUDIOPLAYERFFMPEGAUDIOREADERNATIVE__EXPORTS BOOL AudioSource_ReadFrame(AudioSource* pAudioSource, AVFrame* pframe);


#endif // !ScrcpyNativeExports_H
