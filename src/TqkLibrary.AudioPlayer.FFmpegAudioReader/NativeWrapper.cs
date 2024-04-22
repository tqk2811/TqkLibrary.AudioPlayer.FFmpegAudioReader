using System.Reflection;
using System.Runtime.InteropServices;

namespace TqkLibrary.AudioPlayer.FFmpegAudioReader
{
    internal static class NativeWrapper
    {
#if NETFRAMEWORK
        static NativeWrapper()
        {
            string path = Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!,
                "runtimes",
                Environment.Is64BitProcess ? "win-x64" : "win-x86",
                "native"
                );
            bool r = SetDllDirectory(path);
            if (!r)
                throw new InvalidOperationException("Can't set Kernel32.SetDllDirectory");
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool SetDllDirectory(string PathName);
#endif

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetLastError();



        [DllImport("TqkLibrary.AudioPlayer.FFmpegAudioReader.Native.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AudioSource_Alloc([MarshalAs(UnmanagedType.LPStr)] string filePath);

        [DllImport("TqkLibrary.AudioPlayer.FFmpegAudioReader.Native.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void AudioSource_Free(ref IntPtr ppAudioSource);

        [DllImport("TqkLibrary.AudioPlayer.FFmpegAudioReader.Native.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool AudioSource_ReadFrame(IntPtr pAudioSource, IntPtr pframe);


        [DllImport("avutil-57.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr av_frame_alloc();
        [DllImport("avutil-57.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_frame_free(ref IntPtr pAVFrame);
    }
}
