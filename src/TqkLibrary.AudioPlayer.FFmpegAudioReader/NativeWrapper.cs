using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TqkLibrary.AudioPlayer.FFmpegAudioReader
{
    internal static class NativeWrapper
    {
        // Single native library the managed code P/Invokes. The FFmpeg shared
        // libraries are dependencies of it and are loaded transitively
        // (see the resolver below on .NET, or SetDllDirectory on .NET Framework).
        internal const string LibName = "TqkLibrary.AudioPlayer.FFmpegAudioReader.Native";

#if NET5_0_OR_GREATER
        static NativeWrapper()
        {
            NativeLibrary.SetDllImportResolver(typeof(NativeWrapper).Assembly, Resolve);
        }

        static IntPtr _wrapperHandle = IntPtr.Zero;
        static readonly object _loadLock = new object();

        static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (!string.Equals(libraryName, LibName, StringComparison.Ordinal))
                return IntPtr.Zero; // not ours; let the default resolver handle it

            if (_wrapperHandle != IntPtr.Zero)
                return _wrapperHandle;

            lock (_loadLock)
            {
                if (_wrapperHandle != IntPtr.Zero)
                    return _wrapperHandle;

                string[] dirs = GetNativeSearchDirectories();

                // Pre-load the FFmpeg dependencies (avutil first). This registers
                // each lib under its SONAME so the wrapper's imports resolve even
                // though Linux ships fully-versioned file names (libavutil.so.60.x.y).
                foreach (string name in new[] { "avutil", "swresample", "swscale", "avcodec", "avformat" })
                    TryPreload(name, dirs);

                string wrapperFile = WrapperFileName();
                foreach (string dir in dirs)
                {
                    string candidate = Path.Combine(dir, wrapperFile);
                    if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out _wrapperHandle))
                        return _wrapperHandle;
                }
            }

            // Could not locate it explicitly; fall back to the default search.
            return IntPtr.Zero;
        }

        static string WrapperFileName()
        {
            if (OperatingSystem.IsWindows())
                return LibName + ".dll";
            if (OperatingSystem.IsMacOS())
                return "lib" + LibName + ".dylib";
            return "lib" + LibName + ".so";
        }

        static void TryPreload(string name, string[] dirs)
        {
            string pattern = OperatingSystem.IsWindows()
                ? name + "-*.dll"
                : OperatingSystem.IsMacOS() ? "lib" + name + ".*dylib" : "lib" + name + ".so*";
            foreach (string dir in dirs)
            {
                string? file = Directory.EnumerateFiles(dir, pattern).FirstOrDefault();
                if (file != null && NativeLibrary.TryLoad(file, out _))
                    return;
            }
        }

        static string[] GetNativeSearchDirectories()
        {
            string rid = GetRuntimeIdentifier();
            string relNative = Path.Combine("runtimes", rid, "native");

            string?[] bases =
            {
                AppContext.BaseDirectory,
                Path.GetDirectoryName(typeof(NativeWrapper).Assembly.Location),
            };

            return bases
                .Where(b => !string.IsNullOrEmpty(b))
                .SelectMany(b => new[] { b!, Path.Combine(b!, relNative) })
                .Where(Directory.Exists)
                .Distinct()
                .ToArray();
        }

        static string GetRuntimeIdentifier()
        {
            string os =
                OperatingSystem.IsWindows() ? "win" :
                OperatingSystem.IsLinux() ? "linux" :
                OperatingSystem.IsMacOS() ? "osx" : "unknown";
            string arch = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => "x86",
                Architecture.X64 => "x64",
                Architecture.Arm => "arm",
                Architecture.Arm64 => "arm64",
                _ => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
            };
            return $"{os}-{arch}";
        }
#elif NETFRAMEWORK
        static NativeWrapper()
        {
            // .NET Framework is Windows-only. Point the OS loader at the
            // runtimes\win-<arch>\native folder that holds the wrapper DLL and
            // its FFmpeg dependencies.
            string? baseDir = Path.GetDirectoryName(typeof(NativeWrapper).Assembly.Location);
            if (!string.IsNullOrEmpty(baseDir))
            {
                string rid = Environment.Is64BitProcess ? "win-x64" : "win-x86";
                string nativeDir = Path.Combine(baseDir!, "runtimes", rid, "native");
                if (Directory.Exists(nativeDir))
                    SetDllDirectory(nativeDir);
            }
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);
#endif

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AudioSource_Alloc(byte[] filePathUtf8);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void AudioSource_Free(ref IntPtr ppAudioSource);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int AudioSource_ReadFrame(IntPtr pAudioSource, IntPtr pframe);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr AudioSource_FrameAlloc();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void AudioSource_FrameFree(ref IntPtr ppFrame);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int AudioSource_GetLastError();
    }
}
