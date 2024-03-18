namespace PythonNETExtensions.Config
{
    public struct DefaultPythonConfig: IPythonConfig<DefaultPythonConfig>
    {
        public static string PythonBundleContainingDirectory => string.Empty;
        public static string PythonBundleDirectoryName => "PythonHome";
    }
}