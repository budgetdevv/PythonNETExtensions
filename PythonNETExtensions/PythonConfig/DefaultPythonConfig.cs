namespace PythonNETExtensions.PythonConfig
{
    public struct DefaultPythonConfig: IPythonConfig<DefaultPythonConfig>
    {
        public static string PythonBundleContainingDirectory => "./";
        public static string PythonBundleDirectoryName => "PythonHome";
    }
}