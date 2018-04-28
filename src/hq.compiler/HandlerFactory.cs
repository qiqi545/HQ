using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace hq.compiler
{
	public class HandlerFactory
    {
	    public delegate object Handler(object target, object[] parameters);

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

	    public HandlerFactory() : this (new DefaultAssemblyBuilder(new DefaultAssemblyLoadContextProvider(), new IMetadataReferenceResolver[] { new DefaultMetadataReferenceResolver() })) { }
		
		public static HandlerFactory Default = new HandlerFactory();

	    public Assembly BuildAssemblyInMemory(HandlerInfo info, params Assembly[] dependencies)
	    {
			var code = info.Code ?? NoCodeHandler;
		    var mergedDependencies = _defaultDependencies.Union(dependencies).Distinct().ToArray();
		    var a = _builder.CreateInMemory(code, mergedDependencies);
		    return a;
	    }

	    public Assembly BuildAssemblyOnDisk(HandlerInfo info, string outputPath, string pdbPath = null, params Assembly[] dependencies)
	    {
		    var code = info.Code ?? NoCodeHandler;
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

		public Handler BuildHandler(HandlerInfo info, DelegateBuildStrategy strategy = DelegateBuildStrategy.MethodInfo, params Assembly[] dependencies)
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

		public MethodInfo BuildHandlerDirect(HandlerInfo info, params Assembly[] dependencies)
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
