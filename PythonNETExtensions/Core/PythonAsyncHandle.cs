﻿using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace PythonNETExtensions.Core
{
    public class AsyncPythonHandle: IDisposable
    {
        private static readonly ConcurrentBag<AsyncPythonHandle> POOL = [];
        
        public static AsyncPythonHandle Create()
        {
            if (POOL.TryTake(out var instance))
            {
                Ctor(instance);
            }

            else
            {
                instance = new AsyncPythonHandle();
            }

            return instance;
            
            [UnsafeAccessor(UnsafeAccessorKind.Method, Name = ".ctor")]
            static extern void Ctor(AsyncPythonHandle c);
        }

        internal PythonHandle Handle;
        
        public AsyncPythonHandle()
        {
            Handle = new PythonHandle();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Handle.Dispose();
            POOL.Add(this);
        }
    }
}