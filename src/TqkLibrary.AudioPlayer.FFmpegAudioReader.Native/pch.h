#ifndef PCH_H
#define PCH_H

#ifdef TQKLIBRARYAUDIOPLAYERFFMPEGAUDIOREADERNATIVE_EXPORTS
#define TQKLIBRARYAUDIOPLAYERFFMPEGAUDIOREADERNATIVE__EXPORTS extern "C" __declspec( dllexport )
#else
#define TQKLIBRARYAUDIOPLAYERFFMPEGAUDIOREADERNATIVE__EXPORTS extern "C" __declspec( dllimport )
#endif


#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <assert.h>
#include "libav.h"
typedef class AudioSource AudioSource;

#ifdef TQKLIBRARYAUDIOPLAYERFFMPEGAUDIOREADERNATIVE_EXPORTS

#include "AudioSource.h"

#endif


#include "Exports.h"

#endif //PCH_H
