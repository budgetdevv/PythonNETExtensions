using System;
using System.Runtime.CompilerServices;

namespace PythonNETExtensions.PythonVersions
{
    public interface IPythonVersion
    {
        public static abstract string VersionString { get; }
        
        public static abstract PlatformEmbeddedPython OSXEmbeddedPython { get; }
        
        public static abstract PlatformEmbeddedPython WindowsEmbeddedPython { get; }
        
        public static abstract PlatformEmbeddedPython LinuxEmbeddedPython { get; }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPythonBundleDownloadURL<PyVersionT>() where PyVersionT: struct, IPythonVersion
        {
            string pythonBundleDownloadURL;
            
            if (OperatingSystem.IsMacOS())
            {
                pythonBundleDownloadURL = PyVersionT.OSXEmbeddedPython.GetDownloadURLForCurrentArch();
            }
            
            else if (OperatingSystem.IsWindows())
            {
                pythonBundleDownloadURL = PyVersionT.WindowsEmbeddedPython.GetDownloadURLForCurrentArch();
            }

            else if (OperatingSystem.IsLinux())
            {
                pythonBundleDownloadURL = PyVersionT.LinuxEmbeddedPython.GetDownloadURLForCurrentArch();
            }

            else
            {
                throw new PlatformNotSupportedException();
            }

            return pythonBundleDownloadURL;
        }
    }
}