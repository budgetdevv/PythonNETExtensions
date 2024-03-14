using System;

namespace PythonNETExtensions.PythonVersions
{
    public readonly struct PyVer3_11: IPythonVersion
    {
        public static string OSXPythonBundleDownloadURL => "https://github.com/budgetdevv/PythonNETExtensions/raw/main/Bundles/3.11/OSX/PythonBundle.zip";
        public static string WindowsPythonBundleDownloadURL => throw new NotImplementedException();
        public static string LinuxPythonBundleDownloadURL => throw new NotImplementedException(); 
    }
}