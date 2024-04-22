#include "pch.h"
#include "AudioSource.h"

AudioSource::AudioSource() {
	pInputFormatContext = avformat_alloc_context();
	assert(pInputFormatContext);
}

AudioSource::~AudioSource() {
	if (this->pInputFormatContext)
	{
		avformat_close_input(&pInputFormatContext);
		avformat_free_context(pInputFormatContext);
		pInputFormatContext = nullptr;
	}
	if (this->pCodecContext)
	{
		avcodec_close(pCodecContext);
		avcodec_free_context(&pCodecContext);
		pCodecContext = nullptr;
	}
}

BOOL AudioSource::Init(LPSTR filePath) {
	int err = avformat_open_input(&pInputFormatContext, filePath, nullptr, nullptr);
	if (err)
		return FALSE;

	err = avformat_find_stream_info(pInputFormatContext, nullptr);
	if (err)
		return FALSE;

	for (int i = 0; i < pInputFormatContext->nb_streams; i++)
	{
		if (audioIndex == -1 && pInputFormatContext->streams[i]->codecpar->codec_type == AVMediaType::AVMEDIA_TYPE_AUDIO)
		{
			audioIndex = i;
		}
	}
	input_audio_stream = pInputFormatContext->streams[audioIndex];
	const AVCodec* audio_codec = avcodec_find_decoder(input_audio_stream->codecpar->codec_id);
	pCodecContext = avcodec_alloc_context3(audio_codec);
	this->pCodecContext->flags |= AV_CODEC_FLAG_LOW_DELAY;
	this->pCodecContext->ch_layout.nb_channels = 2;
	this->pCodecContext->sample_rate = 44100;
	err = avcodec_open2(this->pCodecContext, audio_codec, nullptr);
	if (err)
		return FALSE;

	return err == 0;
}
BOOL AudioSource::ReadFrame(AVFrame* pframe) {
	if (!pframe)
		return FALSE;

	av_frame_unref(pframe);

	AVPacket pkt;
	while (true)
	{
		int err = av_read_frame(pInputFormatContext, &pkt);
		if (err == AVERROR_EOF)
			return FALSE;

		//if (pkt.pts == AV_NOPTS_VALUE)
		//	continue;

		if (pkt.stream_index == audioIndex)
		{
			err = CheckErr(avcodec_send_packet(pCodecContext, &pkt));
			if (!err)
				err = CheckErr(avcodec_receive_frame(pCodecContext, pframe));

			av_packet_unref(&pkt);
			return err == 0;
		}
		av_packet_unref(&pkt);
	}
}