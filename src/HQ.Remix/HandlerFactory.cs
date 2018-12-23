#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using HQ.Remix.Internal;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace HQ.Remix
{
    public class HandlerFactory
    {
        private const string NoCSharpCodeHandler = @"
namespace HelloWorld
{ 
    public class Main
    { 
        public static string Execute()
        { 
            return ""Hello, World!"";
        }
    }
}";

        private const string NoJavaScriptCodeHandler = @"
function(callback) { 
  var result = 'Hello, World!';
  callback(null, result); 
};";

        public static HandlerFactory Default = new HandlerFactory();

        private readonly IAssemblyBuilder _builder;
        private readonly IEnumerable<Assembly> _defaultDependencies;
        private readonly INodeServices _nodeServices;

        public HandlerFactory(IAssemblyBuilder builder, INodeServices nodeServices,
            IEnumerable<Assembly> defaultDependencies)
        {
            _builder = builder;
            _nodeServices = nodeServices;
            _defaultDependencies = defaultDependencies ?? GetRuntimeAssemblies();
        }

        public HandlerFactory(IAssemblyBuilder builder, INodeServices nodeServices,
            params Assembly[] defaultDependencies) : this(builder, nodeServices, defaultDependencies.AsEnumerable())
        {
        }

        public HandlerFactory(INodeServices nodeServices,
            params Assembly[] defaultDependencies) : this(AssemblyBuilder.Default.Value, nodeServices,
            defaultDependencies.AsEnumerable())
        {
        }

        public HandlerFactory() : this(AssemblyBuilder.Default.Value,
            NodeServicesFactory.CreateNodeServices(DefaultNodeServicesOptions()))
        {
        }

        private static NodeServicesOptions DefaultNodeServicesOptions()
        {
            var options = new NodeServicesOptions(new ServiceCollection().BuildServiceProvider())
            {
                ProjectPath = Directory.GetCurrentDirectory()
            };
            return options;
        }

        public Assembly BuildAssemblyInMemory(string assemblyName, HandlerInfo info, params Assembly[] dependencies)
        {
            var code = info.Code ?? NoCSharpCodeHandler;
            var mergedDependencies = _defaultDependencies.Union(dependencies).Distinct().ToArray();
            var a = _builder.CreateInMemory(assemblyName, code, mergedDependencies);
            return a;
        }

        public Assembly BuildAssemblyOnDisk(string assemblyName, HandlerInfo info, string outputPath,
            string pdbPath = null, params Assembly[] dependencies)
        {
            var code = info.Code ?? NoCSharpCodeHandler;
            var mergedDependencies = _defaultDependencies.Union(dependencies).Distinct().ToArray();
            var a = _builder.CreateOnDisk(assemblyName, code, outputPath, pdbPath, mergedDependencies);
            return a;
        }

        public Type BuildType(string assemblyName, HandlerInfo info, Assembly[] dependencies)
        {
            var a = BuildAssemblyInMemory(assemblyName, info, dependencies);
            var entrypoint = info.Entrypoint ?? $"{info.Namespace ?? "HelloWorld"}.Main";
            var t = a?.GetType(entrypoint);
            return t;
        }

        public Handler BuildCSharpHandler(string assemblyName, HandlerInfo info,
            DelegateBuildStrategy strategy = DelegateBuildStrategy.MethodInfo, params Assembly[] dependencies)
        {
            var type = BuildType(assemblyName, info, dependencies);
            var function = info.Function ?? "Execute";
            var methodInfo = type?.GetMethod(function, BindingFlags.Public | BindingFlags.Static);
            return methodInfo == null ? null : FromMethod(type, methodInfo, strategy);
        }

        public MethodInfo BuildCSharpHandlerDirect(string assemblyName, HandlerInfo info,
            params Assembly[] dependencies)
        {
            var t = BuildType(assemblyName, info, dependencies);
            var function = info.Function ?? "Execute";
            var method = t?.GetMethod(function, BindingFlags.Public | BindingFlags.Static);
            return method;
        }

        public Handler BuildJavaScriptHandler<T>(HandlerInfo info)
        {
            var @namespace = info.Namespace ?? "module";
            var entrypoint = info.Entrypoint ?? "exports";
            var code = info.Code ?? $"{@namespace}.{entrypoint} = {NoJavaScriptCodeHandler}";

            var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(code);
            var hash = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var c in hash)
                sb.Append(c.ToString("X2"));
            var moduleName = $"{sb}.js";

            File.WriteAllText(moduleName, code);
            return (t, p) => _nodeServices.InvokeAsync<T>(moduleName, p).Result;
        }

        private static IEnumerable<Assembly> GetRuntimeAssemblies()
        {
            var dependencies = DependencyContext.Default.RuntimeLibraries
                .SelectMany(info => info.Dependencies);

            var assemblies = dependencies
                .Select(info => Assembly.Load(info.Name));

            return assemblies;
        }

        #region Method Handler Factory

        private readonly IDictionary<MethodAndStrategy, Handler> _handlers =
            new ConcurrentDictionary<MethodAndStrategy, Handler>();

        private struct MethodAndStrategy : IEquatable<MethodAndStrategy>
        {
            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_methodInfo != null ? _methodInfo.GetHashCode() : 0) * 397) ^ (int) _strategy;
                }
            }

            public MethodAndStrategy(MethodInfo methodInfo, DelegateBuildStrategy strategy)
            {
                _methodInfo = methodInfo;
                _strategy = strategy;
            }

            private readonly MethodInfo _methodInfo;
            private readonly DelegateBuildStrategy _strategy;

            public override bool Equals(object obj)
            {
                return obj is MethodAndStrategy strategy && Equals(strategy);
            }

            public bool Equals(MethodAndStrategy other)
            {
                return EqualityComparer<MethodInfo>.Default.Equals(_methodInfo, other._methodInfo) &&
                       _strategy == other._strategy;
            }

            public static bool operator ==(MethodAndStrategy left, MethodAndStrategy right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(MethodAndStrategy left, MethodAndStrategy right)
            {
                return !(left == right);
            }
        }

        public Handler GetOrCacheHandlerFromMethod(Type type, MethodInfo methodInfo, DelegateBuildStrategy strategy)
        {
            var key = new MethodAndStrategy(methodInfo, strategy);
            if (!_handlers.TryGetValue(key, out var handler))
                _handlers[key] = handler = FromMethod(type, methodInfo, strategy);
            return handler;
        }

        public static Handler FromMethod(Type type, MethodInfo methodInfo, DelegateBuildStrategy strategy)
        {
            Contract.Assert(methodInfo != null);
            Contract.Assert(type != null);

            switch (strategy)
            {
                case DelegateBuildStrategy.MethodInfo:
                {
                    return methodInfo.Invoke;
                }
                case DelegateBuildStrategy.Expression:
                {
                    var parameters = methodInfo.GetParameters();
                    var arguments = parameters.Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToList();
                    var methodCall =
                        Expression.Call(methodInfo.IsStatic ? null : Expression.Parameter(typeof(object), "instance"),
                            methodInfo, arguments);
                    var lambda = Expression.Lambda(Expression.Convert(methodCall, typeof(object)), arguments);
                    var d = lambda.Compile();
                    return methodInfo.IsStatic
                        ? (Handler) ((t, p) => d.DynamicInvoke(p))
                        : (t, p) => d.DynamicInvoke(t, p);
                }
                case DelegateBuildStrategy.ObjectExecutor:
                {
                    var executor = ObjectMethodExecutor.Create(methodInfo, type.GetTypeInfo());
                    return (t, p) => executor.Execute(t, p);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
            }
        }

        #endregion
    }
}
