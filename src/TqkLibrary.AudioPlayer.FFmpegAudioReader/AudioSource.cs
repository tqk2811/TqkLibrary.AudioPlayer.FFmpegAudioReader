using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TqkLibrary.AudioPlayer.FFmpegAudioReader
{
    public class AudioSource : IDisposable
    {
        readonly string _filePath;
        IntPtr _pointer = IntPtr.Zero;
        public AudioSource(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);
            _filePath = filePath;

            _pointer = NativeWrapper.AudioSource_Alloc(filePath);
            if (_pointer == IntPtr.Zero)
                throw new ApplicationException($"Create and load {nameof(AudioSource)} failed (last error : {NativeWrapper.GetLastError()})");
        }
        ~AudioSource()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        void Dispose(bool disposing)
        {
            if (_pointer != IntPtr.Zero)
            {
                NativeWrapper.AudioSource_Free(ref _pointer);
            }
        }

        public bool ReadFrame(AVFrame aVFrame)
        {
            return NativeWrapper.AudioSource_ReadFrame(_pointer, aVFrame.Handle);
        }

        public class AVFrame : IDisposable
        {
            IntPtr _handle = IntPtr.Zero;
            public IntPtr Handle { get { return _handle; } }
            public AVFrame()
            {
                _handle = NativeWrapper.av_frame_alloc();
            }
            ~AVFrame()
            {
                Dispose(false);
            }
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            void Dispose(bool disposing)
            {
                if (_handle != IntPtr.Zero)
                {
                    NativeWrapper.av_frame_free(ref _handle);
                }
            }
        }
    }
}
