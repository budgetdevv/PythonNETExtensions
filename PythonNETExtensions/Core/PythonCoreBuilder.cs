using PythonNETExtensions.Config;
using PythonNETExtensions.Versions;

namespace PythonNETExtensions.Core;

public struct PythonCoreBuilder
{
    public _1<PyConfigT> WithConfig<PyConfigT>() where PyConfigT : struct, IPythonConfig<PyConfigT>
    {
        return new _1<PyConfigT>();
    }
        
    public struct _1<PyConfigT> where PyConfigT : struct, IPythonConfig<PyConfigT>
    {
        public _2<PyVersionT> WithVersion<PyVersionT>() where PyVersionT : struct, IPythonVersion<PyVersionT, PyConfigT>
        {
            return new _2<PyVersionT>();
        }
            
        public struct _2<PyVersionT> where PyVersionT : struct, IPythonVersion<PyVersionT, PyConfigT>
        {
            public PythonCore<PyVersionT, PyConfigT> Build()
            {
                return PythonCore<PyVersionT, PyConfigT>.INSTANCE;
            }
        }
    }
}