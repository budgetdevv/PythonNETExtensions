namespace PythonNETExtensions.Modules;

public struct MainModule: IPythonModule
{
    public static string[] DependentPackages { get; } = [];
    public static string ModuleName => "__main__";
}