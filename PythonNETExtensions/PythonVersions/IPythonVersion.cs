using System;
using System.Runtime.CompilerServices;

namespace PythonNETExtensions.PythonVersions
{
    public interface IPythonVersion<PyVersionT> where PyVersionT: struct, IPythonVersion<PyVersionT>
    {
        public static abstract string VersionString { get; }
        
        public static abstract PlatformEmbeddedPython OSXEmbeddedPython { get; }
        
        public static abstract PlatformEmbeddedPython WindowsEmbeddedPython { get; }
        
        public static abstract PlatformEmbeddedPython LinuxEmbeddedPython { get; }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static virtual PlatformEmbeddedPython GetPlatformEmbeddable()
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static virtual string GetDLLPath(string homePath)
        {
            if (OperatingSystem.IsMacOS())
            {
                return $"{homePath}lib/libpython{PyVersionT.VersionString}.dylib";
            }
            
            else if (OperatingSystem.IsWindows())
            {
                return $"{homePath}python{PyVersionT.VersionString.Replace(".", string.Empty)}.dll";
            }
            
            else if (OperatingSystem.IsLinux())
            {
                return $"{homePath}lib/libpython{PyVersionT.VersionString}.so";
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static virtual string GetPipExecutablePath(string homePath)
        {
            if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            {
                return $"{homePath}bin/pip3";
            }

            else if (OperatingSystem.IsWindows())
            {
                // TODO: Implement
            }
            
            throw new PlatformNotSupportedException();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static virtual string GetPackagesPath(string homePath)
        {
            if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            {
                return $"{homePath}lib/python{PyVersionT.VersionString}/site-packages";
            }

            else if (OperatingSystem.IsWindows())
            {
                // TODO: Implement
            }
            
            throw new PlatformNotSupportedException();
        }
    }
}