using System.IO;

namespace PythonNETExtensions.PythonConfig
{
    public interface IPythonConfig<PyConfigT> where PyConfigT: struct, IPythonConfig<PyConfigT>
    {
        public static abstract string PythonBundleContainingDirectory { get; }
        
        public static abstract string PythonBundleDirectoryName { get; }

        public static virtual string PythonHomePath => Path.Combine(PyConfigT.PythonBundleContainingDirectory, PyConfigT.PythonBundleDirectoryName);
    }
}