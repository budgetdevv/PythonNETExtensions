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
        public static PlatformEmbeddedPython GetPlatformEmbeddable<PyVersionT>() where PyVersionT: struct, IPythonVersion
        {
            if (OperatingSystem.IsMacOS())
            {
                return PyVersionT.OSXEmbeddedPython;
            }
            
            else if (OperatingSystem.IsWindows())
            {
                return PyVersionT.WindowsEmbeddedPython;
            }

            else if (OperatingSystem.IsLinux())
            {
                return PyVersionT.LinuxEmbeddedPython;
            }

            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}