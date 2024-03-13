using System;
using System.Runtime.CompilerServices;

namespace PythonNETExtensions.PythonVersions
{
    public interface IPythonVersion
    {
        public static abstract string OSXPythonBundleDownloadURL { get; }
        public static abstract string WindowsPythonBundleDownloadURL { get; }
        public static abstract string LinuxPythonBundleDownloadURL { get; }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPythonBundleDownloadURL<PyVersionT>() where PyVersionT: struct, IPythonVersion
        {
            string pythonBundleDownloadURL;
            
            if (OperatingSystem.IsMacOS())
            {
                pythonBundleDownloadURL = PyVersionT.OSXPythonBundleDownloadURL;
            }
            
            else if (OperatingSystem.IsWindows())
            {
                pythonBundleDownloadURL = PyVersionT.WindowsPythonBundleDownloadURL;
            }

            else if (OperatingSystem.IsLinux())
            {
                pythonBundleDownloadURL = PyVersionT.LinuxPythonBundleDownloadURL;
            }

            else
            {
                throw new PlatformNotSupportedException();
            }

            return pythonBundleDownloadURL;
        }
    }
}