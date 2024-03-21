using System;

namespace PythonNETExtensions.Core.Handles
{
    public interface IPythonHandle: IDisposable
    {
        public LongRunningCSharpRegion GetLongRunningCSharpRegion();
    }
}