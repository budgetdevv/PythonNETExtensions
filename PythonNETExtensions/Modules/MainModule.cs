namespace PythonNETExtensions.Modules;

public struct MainModule: IPythonModule
{
    public static string[] DependentPackage => null;
    public static string ModuleName => "__main__";
}