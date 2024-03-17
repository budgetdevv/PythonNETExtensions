namespace PythonNETExtensions.Modules.PythonBuiltIn
{
    public struct MainModule: IPythonBuiltInModule<MainModule>
    {
        public static string ModuleName => "__main__";
    }
}