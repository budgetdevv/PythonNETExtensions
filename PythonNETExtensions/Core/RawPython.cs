using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using Python.Runtime;
using PythonNETExtensions.Helpers;

namespace PythonNETExtensions.Core
{
    public static class RawPython
    {
        public enum CompilationOption
        {
            Auto,
            Compile,
            ExecuteOnly
        }
        
        public interface IRunOptions
        {
            public static abstract CompilationOption CompilationOption { get; }
            public static abstract bool UseCachedScope { get; }
        }

        public struct DefaultRunOptions: IRunOptions
        {
            public static CompilationOption CompilationOption => CompilationOption.Auto;
            public static bool UseCachedScope => false;
        }
        
        public struct CachedScopeRunOptions: IRunOptions
        {
            public static CompilationOption CompilationOption => CompilationOption.Auto;
            
            // Use new module, this is so that interpolated variables are "captured".
            public static bool UseCachedScope => true;
        }
        
        private struct NoRet { }
        
        private struct StringBuilderThreadStaticDefinition: IThreadStaticDefinition<StringBuilder>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static StringBuilder CreateItem()
            {
                return new StringBuilder();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void OnGet(ref StringBuilder item)
            {
                item.Clear();
            }
        }
        
        private struct PyScopeThreadStaticDefinition: IThreadStaticDefinition<PyModule>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PyModule CreateItem()
            {
                return Py.CreateScope();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void OnGet(ref PyModule item) { }
        }

        [InterpolatedStringHandler]
        public struct CodeInterpolator<OptsT> where OptsT: IRunOptions
        {
            private readonly StringBuilder LocalStringBuilder;

            public readonly PyModule Scope;

            private int CurrentObjectIndex;

            public bool ShouldCompile;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CodeInterpolator(int literalLength, int formattedCount)
            {
                // StringBuilderThreadStaticDefinition clears the StringBuilder for us
                var stringBuilder = LocalStringBuilder = ThreadStatic<StringBuilderThreadStaticDefinition, StringBuilder>.Item;
                stringBuilder.EnsureCapacity(literalLength);
                
                Scope = OptsT.UseCachedScope ? ThreadStatic<PyScopeThreadStaticDefinition, PyModule>.Item : Py.CreateScope("NoCache");
                ShouldCompile = OptsT.CompilationOption != CompilationOption.ExecuteOnly;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendLiteral(string text)
            {
                LocalStringBuilder.Append(text);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendFormatted(string text)
            {
                // If any of the formatted strings are not interned, we shouldn't compile
                if (OptsT.CompilationOption == CompilationOption.Auto && string.IsInterned(text) == null)
                {
                    ShouldCompile = false;
                }
                
                LocalStringBuilder.Append(text);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendFormatted<T>(T item)
            {
                LocalStringBuilder.Append(item);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendFormatted(dynamic item, string format)
            {
                AppendFormatted<dynamic>(item, format);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendFormatted<T>(T item, string format)
            {
                // JIT should be able to eliminate branching, since format is a constant string.
                if (format == "py")
                {
                    var name = $"py_local_{CurrentObjectIndex++}";
                    Scope.Set(name, item);
                    LocalStringBuilder.Append(name);
                }

                else
                {
                    throw new Exception("Invalid format");
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
            {
                return LocalStringBuilder.ToString();
            }
        }

        private static readonly ConcurrentDictionary<string, PyObject> CODE_TO_COMPILATION_MAP = new(); 
        
        private const string RET_VAR_NAME = "py_ret";

        public static void Run(CodeInterpolator<DefaultRunOptions> code)
        {
            Run<NoRet, DefaultRunOptions>(code);
        }
        
        public static void RunWithCachedScope(CodeInterpolator<CachedScopeRunOptions> code)
        {
            Run<NoRet, CachedScopeRunOptions>(code);
        }
        
        public static RetT Run<RetT>(CodeInterpolator<DefaultRunOptions> code)
        {
            return Run<RetT, DefaultRunOptions>(code);
        }
        
        public static RetT RunWithCachedScope<RetT>(CodeInterpolator<CachedScopeRunOptions> code)
        {
            return Run<RetT, CachedScopeRunOptions>(code);
        }
        
        public static RetT Run<RetT, OptsT>(CodeInterpolator<OptsT> code)
            where OptsT: IRunOptions
        {
            var hasRet = typeof(RetT) != typeof(NoRet);
            
            var codeText = code.ToString();

            const string METHOD_WRAPPER_NAME = "py_wrapper_method";
            
            // TODO: Somehow optimize performance of this
            codeText = hasRet ?
            $"""
            def {METHOD_WRAPPER_NAME}():
            {codeText.IndentCode()}
                
            {RET_VAR_NAME} = {METHOD_WRAPPER_NAME}();
            """ : codeText;
            
            var scope = code.Scope;
            
            var shouldCompile = code.ShouldCompile;

            if (false)
            {
                Console.WriteLine(
                    $"""
                     Compiled: {shouldCompile}

                     Codegen:
                     {codeText}
                     
                     Scope: {scope}
                        Variables: {scope.Variables().Keys()}
                     """);
            }
            
            if (shouldCompile)
            {
                var compilations = CODE_TO_COMPILATION_MAP;

                if (!compilations.TryGetValue(codeText, out var compilation))
                {
                    compilations[codeText] = compilation = PythonEngine.Compile(codeText);
                }

                scope.Execute(compilation);
            }

            else
            {
                scope.Exec(codeText);
            }
            
            var ret = scope.Get<RetT>(RET_VAR_NAME);

            if (OptsT.UseCachedScope)
            {
                scope.Variables().Clear();
            }

            return ret;
        }
    }
}