// Decompiled with JetBrains decompiler
// Type: hq.platform.InstanceFactory
// Assembly: hq.platform, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: AD23095B-C5CD-42A2-B496-1FB560BA52AA
// Assembly location: D:\Drive\HQ.IO\src\Archive\Reconstructions\Decompile\hq.platform.1.0.1\hq.platform.dll

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace hq.platform
{
  public static class InstanceFactory
  {
    private static readonly IDictionary<Type, InstanceFactory.ParameterlessObjectActivator> _emptyActivators = (IDictionary<Type, InstanceFactory.ParameterlessObjectActivator>) new ConcurrentDictionary<Type, InstanceFactory.ParameterlessObjectActivator>();
    private static readonly IDictionary<Type, InstanceFactory.ObjectActivator> _activators = (IDictionary<Type, InstanceFactory.ObjectActivator>) new ConcurrentDictionary<Type, InstanceFactory.ObjectActivator>();
    private static readonly IDictionary<Type, ConstructorInfo> _constructors = (IDictionary<Type, ConstructorInfo>) new ConcurrentDictionary<Type, ConstructorInfo>();
    private static readonly IDictionary<ConstructorInfo, ParameterInfo[]> _constructorParameters = (IDictionary<ConstructorInfo, ParameterInfo[]>) new ConcurrentDictionary<ConstructorInfo, ParameterInfo[]>();

    public static object CreateInstance<T>(object example)
    {
      return InstanceFactory.CreateInstance(example.GetType());
    }

    public static T CreateInstance<T>()
    {
      return (T) InstanceFactory.CreateInstance(typeof (T));
    }

    public static object CreateInstance(Type implementationType)
    {
      InstanceFactory.ParameterlessObjectActivator parameterlessObjectActivator;
      if (!InstanceFactory._emptyActivators.TryGetValue(implementationType, out parameterlessObjectActivator))
      {
        ConstructorInfo constructor = TypeExtensions.GetConstructor(implementationType, Type.EmptyTypes);
        InstanceFactory._emptyActivators[implementationType] = parameterlessObjectActivator = InstanceFactory.DynamicMethodFactory.Build(implementationType, constructor);
      }
      return parameterlessObjectActivator();
    }

    public static object CreateInstance(Type implementationType, object[] args)
    {
      if (args == null || args.Length == 0)
        return InstanceFactory.CreateInstance(implementationType);
      InstanceFactory.ObjectActivator objectActivator;
      if (!InstanceFactory._activators.TryGetValue(implementationType, out objectActivator))
      {
        ConstructorInfo widestConstructor;
        if (!InstanceFactory._constructors.TryGetValue(implementationType, out widestConstructor))
          InstanceFactory._constructors[implementationType] = widestConstructor = InstanceFactory.GetWidestConstructor(implementationType);
        ParameterInfo[] parameters;
        if (!InstanceFactory._constructorParameters.TryGetValue(widestConstructor, out parameters))
          InstanceFactory._constructorParameters[widestConstructor] = parameters = widestConstructor.GetParameters();
        InstanceFactory._activators[implementationType] = objectActivator = InstanceFactory.DynamicMethodFactory.Build(implementationType, widestConstructor, (IReadOnlyList<ParameterInfo>) parameters);
      }
      return objectActivator(args);
    }

    private static ConstructorInfo GetWidestConstructor(Type implementationType)
    {
      return ((IEnumerable<ConstructorInfo>) TypeExtensions.GetConstructors(implementationType)).OrderByDescending<ConstructorInfo, int>((Func<ConstructorInfo, int>) (c => c.GetParameters().Length)).First<ConstructorInfo>();
    }

    private delegate object ParameterlessObjectActivator();

    private delegate object ObjectActivator(params object[] parameters);

    private static class DynamicMethodFactory
    {
      public static InstanceFactory.ParameterlessObjectActivator Build(Type implementationType, ConstructorInfo ctor)
      {
        DynamicMethod dynamicMethod = new DynamicMethod(string.Format("{0}.0ctor", (object) implementationType.FullName), implementationType, Type.EmptyTypes, true);
        ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
        ilGenerator.Emit(OpCodes.Nop);
        ilGenerator.Emit(OpCodes.Newobj, ctor);
        ilGenerator.Emit(OpCodes.Ret);
        return (InstanceFactory.ParameterlessObjectActivator) dynamicMethod.CreateDelegate(typeof (InstanceFactory.ParameterlessObjectActivator));
      }

      public static InstanceFactory.ObjectActivator Build(Type implementationType, ConstructorInfo ctor, IReadOnlyList<ParameterInfo> parameters)
      {
        DynamicMethod dynamicMethod = new DynamicMethod(string.Format("{0}.ctor", (object) implementationType.FullName), implementationType, new Type[1]
        {
          typeof (object[])
        });
        ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
        for (int index = 0; index < parameters.Count; ++index)
        {
          ilGenerator.Emit(OpCodes.Ldarg_0);
          switch (index)
          {
            case 0:
              ilGenerator.Emit(OpCodes.Ldc_I4_0);
              break;
            case 1:
              ilGenerator.Emit(OpCodes.Ldc_I4_1);
              break;
            case 2:
              ilGenerator.Emit(OpCodes.Ldc_I4_2);
              break;
            case 3:
              ilGenerator.Emit(OpCodes.Ldc_I4_3);
              break;
            case 4:
              ilGenerator.Emit(OpCodes.Ldc_I4_4);
              break;
            case 5:
              ilGenerator.Emit(OpCodes.Ldc_I4_5);
              break;
            case 6:
              ilGenerator.Emit(OpCodes.Ldc_I4_6);
              break;
            case 7:
              ilGenerator.Emit(OpCodes.Ldc_I4_7);
              break;
            case 8:
              ilGenerator.Emit(OpCodes.Ldc_I4_8);
              break;
            default:
              ilGenerator.Emit(OpCodes.Ldc_I4, index);
              break;
          }
          ilGenerator.Emit(OpCodes.Ldelem_Ref);
          Type parameterType = parameters[index].ParameterType;
          ilGenerator.Emit(parameterType.GetTypeInfo().IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, parameterType);
        }
        ilGenerator.Emit(OpCodes.Newobj, ctor);
        ilGenerator.Emit(OpCodes.Ret);
        return (InstanceFactory.ObjectActivator) dynamicMethod.CreateDelegate(typeof (InstanceFactory.ObjectActivator));
      }
    }
  }
}
