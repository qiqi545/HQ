using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;

namespace hq.compiler
{
	public class HandlerFactory
    {
	    public delegate object Handler(object target, object[] parameters);

		private readonly IAssemblyBuilder _builder;
	    private readonly INodeServices _nodeServices;
	    private readonly IEnumerable<Assembly> _defaultDependencies;

	    public HandlerFactory(
		    IAssemblyBuilder builder,
		    INodeServices nodeServices,
			IEnumerable<Assembly> defaultDependencies)
        {
            _builder = builder;
	        _nodeServices = nodeServices;
	        _defaultDependencies = defaultDependencies ?? Runtime.GetRuntimeAssemblies();
        }

        public HandlerFactory(IAssemblyBuilder builder, INodeServices nodeServices, params Assembly[] defaultDependencies)
        {
            _builder = builder;
	        _nodeServices = nodeServices;
	        _defaultDependencies = defaultDependencies ?? Runtime.GetRuntimeAssemblies();
        }

	    public HandlerFactory() : this (
		    new DefaultAssemblyBuilder(new DefaultAssemblyLoadContextProvider(), new IMetadataReferenceResolver[] { new DefaultMetadataReferenceResolver() }),
		    NodeServicesFactory.CreateNodeServices(DefaultNodeServicesOptions())) { }

	    private static NodeServicesOptions DefaultNodeServicesOptions()
	    {
		    var options = new NodeServicesOptions(new ServiceCollection().BuildServiceProvider())
		    {
			    ProjectPath = Directory.GetCurrentDirectory()
		    };
			return options;
	    }

	    public static HandlerFactory Default = new HandlerFactory();

	    public Assembly BuildAssemblyInMemory(HandlerInfo info, params Assembly[] dependencies)
	    {
			var code = info.Code ?? NoCSharpCodeHandler;
		    var mergedDependencies = _defaultDependencies.Union(dependencies).Distinct().ToArray();
		    var a = _builder.CreateInMemory(code, mergedDependencies);
		    return a;
	    }

	    public Assembly BuildAssemblyOnDisk(HandlerInfo info, string outputPath, string pdbPath = null, params Assembly[] dependencies)
	    {
		    var code = info.Code ?? NoCSharpCodeHandler;
		    var mergedDependencies = _defaultDependencies.Union(dependencies).Distinct().ToArray();
		    var a = _builder.CreateOnDisk(code, outputPath, pdbPath, mergedDependencies);
		    return a;
	    }

		public Type BuildType(HandlerInfo info, Assembly[] dependencies)
	    {
		    var a = BuildAssemblyInMemory(info, dependencies);
		    var entrypoint = info.Entrypoint ?? $"{info.Namespace ?? "hq"}.Main";
		    var t = a?.GetType(entrypoint);
		    return t;
	    }

		public Handler BuildCSharpHandler(HandlerInfo info, DelegateBuildStrategy strategy = DelegateBuildStrategy.MethodInfo, params Assembly[] dependencies)
		{
			var type = BuildType(info, dependencies);
			var function = info.Function ?? "Execute";
			var methodInfo = type?.GetMethod(function, BindingFlags.Public | BindingFlags.Static);
			if (methodInfo == null)
				return null;

			switch (strategy)
			{
				case DelegateBuildStrategy.MethodInfo:
				{
					return (t, p) => methodInfo.Invoke(t, p);
				}
				case DelegateBuildStrategy.Expression:
				{
					var parameters = methodInfo.GetParameters();
					var arguments = parameters.Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToList();
					var methodCall = Expression.Call(methodInfo.IsStatic ? null : Expression.Parameter(typeof(object), "instance"), methodInfo, arguments);
					var lambda = Expression.Lambda(Expression.Convert(methodCall, typeof(object)), arguments);
					var d = lambda.Compile();
					return methodInfo.IsStatic ? (Handler) ((t, p) => d.DynamicInvoke(p)) : ((t, p) => d.DynamicInvoke(t, p));
				}
				case DelegateBuildStrategy.Emit:
				{
					// Source: https://www.codeproject.com/Articles/14593/A-General-Fast-Method-Invoker

					void EmitFastInt(ILGenerator g, int value)
					{
						switch (value)
						{
							case -1:
								g.Emit(OpCodes.Ldc_I4_M1);
								break;
							case 0:
								g.Emit(OpCodes.Ldc_I4_0);
								break;
							case 1:
								g.Emit(OpCodes.Ldc_I4_1);
								break;
							case 2:
								g.Emit(OpCodes.Ldc_I4_2);
								break;
							case 3:
								g.Emit(OpCodes.Ldc_I4_3);
								break;
							case 4:
								g.Emit(OpCodes.Ldc_I4_4);
								break;
							case 5:
								g.Emit(OpCodes.Ldc_I4_5);
								break;
							case 6:
								g.Emit(OpCodes.Ldc_I4_6);
								break;
							case 7:
								g.Emit(OpCodes.Ldc_I4_7);
								break;
							case 8:
								g.Emit(OpCodes.Ldc_I4_8);
								break;
						}

						if (value > -129 && value < 128)
						{
							g.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
						}
						else
						{
							g.Emit(OpCodes.Ldc_I4, value);
						}
					}

					var dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);
					var il = dynamicMethod.GetILGenerator();
					var parameters = methodInfo.GetParameters();
					var parameterTypes = new Type[parameters.Length];
					for (var i = 0; i < parameterTypes.Length; i++)
					{
						if (parameters[i].ParameterType.IsByRef)
							parameterTypes[i] = parameters[i].ParameterType.GetElementType();
						else
							parameterTypes[i] = parameters[i].ParameterType;
					}
					var locals = new LocalBuilder[parameterTypes.Length];
					for (var i = 0; i < parameterTypes.Length; i++)
					{
						locals[i] = il.DeclareLocal(parameterTypes[i], true);
					}
					for (var i = 0; i < parameterTypes.Length; i++)
					{
						il.Emit(OpCodes.Ldarg_1);
						EmitFastInt(il, i);
						
							il.Emit(OpCodes.Ldelem_Ref);
						il.Emit(parameterTypes[i].IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, parameterTypes[i]);
						il.Emit(OpCodes.Stloc, locals[i]);
					}
					if (!methodInfo.IsStatic)
					{
						il.Emit(OpCodes.Ldarg_0);
					}
					for (var i = 0; i < parameterTypes.Length; i++)
					{
						il.Emit(parameters[i].ParameterType.IsByRef ? OpCodes.Ldloca_S : OpCodes.Ldloc, locals[i]);
					}
					il.EmitCall(methodInfo.IsStatic ? OpCodes.Call : OpCodes.Callvirt, methodInfo, null);
					if (methodInfo.ReturnType == typeof(void))
					{
						il.Emit(OpCodes.Ldnull);
					}
					else
					{
						if (methodInfo.ReturnType.IsValueType)
						{
							il.Emit(OpCodes.Box, methodInfo.ReturnType);
						}
					}
					for (var i = 0; i < parameterTypes.Length; i++)
					{
						if (!parameters[i].ParameterType.IsByRef)
							continue;
						il.Emit(OpCodes.Ldarg_1);
						EmitFastInt(il, i);
						il.Emit(OpCodes.Ldloc, locals[i]);
						if (locals[i].LocalType.IsValueType)
							il.Emit(OpCodes.Box, locals[i].LocalType);
						il.Emit(OpCodes.Stelem_Ref);
					}
					il.Emit(OpCodes.Ret);
					return (Handler)dynamicMethod.CreateDelegate(typeof(Handler));
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
			}
		}

		public MethodInfo BuildCSharpHandlerDirect(HandlerInfo info, params Assembly[] dependencies)
	    {
		    var t = BuildType(info, dependencies);
		    var function = info.Function ?? "Execute";
		    var method = t?.GetMethod(function, BindingFlags.Public | BindingFlags.Static);
		    return method;
	    }

	    public enum DelegateBuildStrategy
	    {
			MethodInfo,
			Expression,
			Emit
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
		    for (var i = 0; i < hash.Length; i++)
			    sb.Append(hash[i].ToString("X2"));
			var moduleName = $"{sb}.js";

			File.WriteAllText(moduleName, code);
		    return (t, p) => _nodeServices.InvokeAsync<string>(moduleName, p).Result;
		}

	    private const string NoCSharpCodeHandler = @"
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

	    private const string NoJavaScriptCodeHandler = @"
function(callback) { 
  var result = 'Hello, World!';
  callback(null, result); 
};";
	}
}
