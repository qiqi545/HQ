using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace hq.compiler
{
    public class HandlerFactory
    {
        private readonly IAssemblyBuilder _builder;
        private readonly IEnumerable<Assembly> _defaultDependencies;

        public HandlerFactory(IAssemblyBuilder builder, IEnumerable<Assembly> defaultDependencies)
        {
            _builder = builder;
            _defaultDependencies = defaultDependencies ?? Runtime.GetRuntimeAssemblies();
        }

        public HandlerFactory(IAssemblyBuilder builder, params Assembly[] defaultDependencies)
        {
            _builder = builder;
            _defaultDependencies = defaultDependencies ?? Runtime.GetRuntimeAssemblies();
        }

        public MethodInfo BuildHandler(HandlerInfo info, params Assembly[] dependencies)
        {
            var code = info.Code ?? NoCodeHandler;
            string entrypoint = info.Entrypoint ?? $"{info.Namespace ?? "hq"}.Main";
            string function = info.Function ?? "Execute";

            Assembly[] mergedDependencies = _defaultDependencies.Union(dependencies).Distinct().ToArray();
            Assembly a = _builder.Create(code, mergedDependencies);
            Type t = a?.GetType(entrypoint);
            MethodInfo h = t?.GetMethod(function, BindingFlags.Public | BindingFlags.Static);

            return h;
        }

        private const string NoCodeHandler = @"
namespace hq
{ 
    public class Main
    { 
        public static string Execute()
        { 
            return ""Hello, World!"";
        }
    }
}";
    }
}
