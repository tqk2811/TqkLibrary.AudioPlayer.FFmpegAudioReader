using System;
using System.IO;
using System.Text;

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

            // Pass the path as a NUL-terminated UTF-8 buffer so it works the same
            // way on Windows and Linux (FFmpeg expects UTF-8 paths).
            byte[] utf8 = Encoding.UTF8.GetBytes(filePath + "\0");
            _pointer = NativeWrapper.AudioSource_Alloc(utf8);
            if (_pointer == IntPtr.Zero)
                throw new ApplicationException($"Create and load {nameof(AudioSource)} failed (last error : {NativeWrapper.AudioSource_GetLastError()})");
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
            return NativeWrapper.AudioSource_ReadFrame(_pointer, aVFrame.Handle) != 0;
        }

        public class AVFrame : IDisposable
        {
            IntPtr _handle = IntPtr.Zero;
            public IntPtr Handle { get { return _handle; } }
            public AVFrame()
            {
                _handle = NativeWrapper.AudioSource_FrameAlloc();
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
                    NativeWrapper.AudioSource_FrameFree(ref _handle);
                }
            }
        }
    }
}
