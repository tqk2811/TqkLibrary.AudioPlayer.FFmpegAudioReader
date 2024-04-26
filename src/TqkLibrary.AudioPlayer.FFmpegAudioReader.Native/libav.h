#ifndef _H_libav_H_
#define _H_libav_H_
extern "C" {
#include <libavcodec/avcodec.h>
#include <libavformat/avformat.h>
#include <libswresample/swresample.h>

#include <libavutil/avutil.h>
}

inline int CheckErr(int err) {
	if (err != 0)
	{
		SetLastError(err);
#if _DEBUG
		char buffer[1024];
		av_strerror(err, buffer, 1024);
		const char* msg = &buffer[0];
		printf(msg);
#endif
	}
	return err;
}
#endif // libav_H
