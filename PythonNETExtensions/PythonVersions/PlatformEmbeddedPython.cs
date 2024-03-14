using System;
using System.Runtime.InteropServices;

namespace PythonNETExtensions.PythonVersions
{
    public struct PlatformEmbeddedPython
    {
        public readonly string DLLPathRelativeToPythonHome;
        
        public readonly string AMD64DownloadURL;

        public readonly string ARM64DownloadURL;

        public PlatformEmbeddedPython(string dllPathRelativeToPythonHome, string amd64DownloadUrl = null, string arm64DownloadUrl = null)
        {
            DLLPathRelativeToPythonHome = dllPathRelativeToPythonHome;
            AMD64DownloadURL = amd64DownloadUrl;
            ARM64DownloadURL = arm64DownloadUrl;
        }

        public string GetDownloadURLForCurrentArch()
        {
            var arch = RuntimeInformation.ProcessArchitecture;

            if (OperatingSystem.IsMacOS())
            {
                if (arch is Architecture.X64 or Architecture.X86 or Architecture.Arm64)
                {
                    // Universal2 binary that supports both archs
                    return AMD64DownloadURL;
                }
            }
            
            else if (OperatingSystem.IsWindows())
            {
                if (arch is Architecture.X64 or Architecture.X86)
                {
                    return AMD64DownloadURL;
                }
            }
            
            else if (OperatingSystem.IsLinux())
            {
                if (arch is Architecture.X64 or Architecture.X86)
                {
                    return AMD64DownloadURL;
                }
                
                else if (arch == Architecture.Arm64)
                {
                    return ARM64DownloadURL;
                }
            }

            throw new PlatformNotSupportedException();
        }
    }
}