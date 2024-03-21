using System;
using System.Runtime.CompilerServices;

namespace PythonNETExtensions.Core
{
    public interface IThreadStaticDefinition<T>
    {
        public static abstract T CreateItem();
        
        public static abstract void OnGet(ref T item);
    }
    
    public static class ThreadStatic<ThreadStaticDefinitionT, ItemT>
        where ThreadStaticDefinitionT: struct, IThreadStaticDefinition<ItemT>
    {
        [ThreadStatic]
        private static ItemT ThreadLocal;
        
        public static ItemT Item
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var item = ThreadLocal ?? CreateItem();
                ThreadStaticDefinitionT.OnGet(ref item);
                return item;

                [MethodImpl(MethodImplOptions.NoInlining)]
                ItemT CreateItem()
                {
                    return ThreadStaticDefinitionT.CreateItem();
                }
            }
        }
    }
}