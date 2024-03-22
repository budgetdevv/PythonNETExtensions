using System;

namespace PythonNETExtensions.Core.Handles
{
    public interface IPythonHandle<HandleT>: IDisposable where HandleT: IPythonHandle<HandleT>
    {
        public static abstract HandleT Create();
        
        public LongRunningCSharpRegion GetLongRunningCSharpRegion();
    }
}