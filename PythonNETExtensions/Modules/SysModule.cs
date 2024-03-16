namespace PythonNETExtensions.Modules
{
    public struct SysModule: IPythonModule<SysModule>
    {
        public static string DependentPackage => null;
        public static string ModuleName => "sys";
    }
}