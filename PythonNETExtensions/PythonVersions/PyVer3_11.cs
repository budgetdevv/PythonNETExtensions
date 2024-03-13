using System;

namespace PythonNETExtensions.PythonVersions
{
    public readonly struct PyVer3_11: IPythonVersion
    {
        public static string OSXPythonBundleDownloadURL => "https://drive.usercontent.google.com/download?id=1v-ddEOnkNszZGhXmh1UQjCvYfRAfS4Kc&export=download&authuser=1&confirm=t&uuid=d9817ef1-7a5f-41ce-8b9d-1cdc88f07641&at=APZUnTUxuSG3nFARelCdTsSvPfoi%3A1710318036536";
        public static string WindowsPythonBundleDownloadURL => throw new NotImplementedException();
        public static string LinuxPythonBundleDownloadURL => throw new NotImplementedException(); 
    }
}