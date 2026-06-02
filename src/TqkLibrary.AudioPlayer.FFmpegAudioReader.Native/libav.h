#ifndef _H_libav_H_
#define _H_libav_H_
#include <cstdio>
extern "C" {
#include <libavcodec/avcodec.h>
#include <libavformat/avformat.h>
#include <libswresample/swresample.h>

#include <libavutil/avutil.h>
}

// Thread-local last FFmpeg error code, exposed to managed code through
// AudioSource_GetLastError(). Replaces the previous Win32 SetLastError/
// GetLastError mechanism so the wrapper works on Linux too.
// inline (C++17) keeps a single definition across translation units.
inline thread_local int g_lastAvError = 0;

inline int CheckErr(int err) {
	if (err != 0)
	{
		g_lastAvError = err;
#if defined(_DEBUG) || !defined(NDEBUG)
		char buffer[1024];
		av_strerror(err, buffer, sizeof(buffer));
		std::printf("%s\n", buffer);
#endif
	}
	return err;
}
#endif // libav_H
