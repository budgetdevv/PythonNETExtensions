using System;

namespace PythonNETExtensions.PythonVersions
{
    public readonly struct PyVer3_11: IPythonVersion
    {
        public static string VersionString => "3.11";
        
        private static readonly string MAC_UNIVERSAL_2DOWNLOAD_URL = $"https://github.com/budgetdevv/PythonNETExtensions/raw/main/Bundles/{VersionString}/OSX/PythonBundle.zip";

        public static PlatformEmbeddedPython OSXEmbeddedPython => new PlatformEmbeddedPython
        (
            amd64DownloadUrl: MAC_UNIVERSAL_2DOWNLOAD_URL,
            arm64DownloadUrl: MAC_UNIVERSAL_2DOWNLOAD_URL
        );

        public static PlatformEmbeddedPython WindowsEmbeddedPython => throw new NotImplementedException();
        
        public static PlatformEmbeddedPython LinuxEmbeddedPython => throw new NotImplementedException();
    }
}