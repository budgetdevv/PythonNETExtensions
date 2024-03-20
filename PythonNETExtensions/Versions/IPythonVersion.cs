using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using PythonNETExtensions.Config;

namespace PythonNETExtensions.Versions
{
    public interface IPythonVersion<PyVersionT, PyConfigT>
        where PyVersionT: struct, IPythonVersion<PyVersionT, PyConfigT>
        where PyConfigT: struct, IPythonConfig<PyConfigT>
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
        public static virtual string GetDLLContainingDirectoryPath()
        {
            var homePath = PyConfigT.PythonHomePath;
            
            if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            {
                return Path.Combine(homePath, "lib");
            }
            
            else if (OperatingSystem.IsWindows())
            {
                return homePath;
            }

            throw new PlatformNotSupportedException();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static virtual string GetDLLPath()
        {
            var dllContainingDirectory = PyVersionT.GetDLLContainingDirectoryPath();

            var version = PyVersionT.VersionString;
            
            if (OperatingSystem.IsMacOS())
            {
                return Path.Combine(dllContainingDirectory, $"libpython{version}.dylib");
            }
    
            else if (OperatingSystem.IsWindows())
            {
                return Path.Combine(dllContainingDirectory, $"python{version.Replace(".", string.Empty)}.dll");
            }
    
            else if (OperatingSystem.IsLinux())
            {
                return Path.Combine(dllContainingDirectory, $"libpython{version}.so");
            }

            throw new PlatformNotSupportedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static virtual string GetPipExecutablePath()
        {
            return Path.Combine(PyConfigT.PythonHomePath, "pip3");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static virtual string GetPackagesPath()
        {
            var homePath = PyConfigT.PythonHomePath;
            
            if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            {
                return Path.Combine(homePath, "lib", $"python{PyVersionT.VersionString}", "site-packages");
            }

            else if (OperatingSystem.IsWindows())
            {
                return Path.Combine(homePath, "Lib", "site-packages");
            }
            
            throw new PlatformNotSupportedException();
        }

        public static virtual void RunWithPythonExecutable(string args)
        {
            var dllContainingDirectory = PyVersionT.GetDLLContainingDirectoryPath();

            var isWindows = OperatingSystem.IsWindows();

            if (!isWindows)
            {
                if (OperatingSystem.IsMacOS())
                {
                    Environment.SetEnvironmentVariable("DYLD_LIBRARY_PATH", $"{dllContainingDirectory}:$DYLD_LIBRARY_PATH");
                }
            
                else if (OperatingSystem.IsLinux())
                {
                    Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", $"{dllContainingDirectory}:$DYLD_LIBRARY_PATH");
                }

                else
                {
                    throw new PlatformNotSupportedException();
                }
            }
            
            var executableName = $"PythonExec{(isWindows ? ".exe" : string.Empty)}";
            
            // Define the process start information
            var startInfo = new ProcessStartInfo
            {
                // FileName = executableName,
                FileName = Path.Combine(Path.GetFullPath(PyConfigT.PythonHomePath), executableName),
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true, 
                // WorkingDirectory = Path.GetFullPath(PyConfigT.PythonHomePath)
            };

            // Create and start the process
            using var process = Process.Start(startInfo)!;
            
            // Read the output
            var output = process.StandardOutput.ReadToEnd();
            var errors = process.StandardError.ReadToEnd();

            process.WaitForExit(); // Wait for the process to exit
                
            if (!string.IsNullOrWhiteSpace(errors))
            {
                 output +=
                 $"""
                 
                 Errors:{errors}
                 """;
            }

            Console.WriteLine(output);
        }
    }
}