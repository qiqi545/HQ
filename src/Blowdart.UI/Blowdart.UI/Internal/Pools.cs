using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using TypeKitchen;

namespace Blowdart.UI.Internal
{
    internal static class Pools
    {
        public static readonly ObjectPool<UiAction> ActionPool = new DefaultObjectPool<UiAction>(new ActionPoolPolicy());
        public static readonly ObjectPool<List<object>> ArgumentsPool = new DefaultObjectPool<List<object>>(new ListObjectPolicy<List<object>>());
        public static readonly ObjectPool<StringBuilder> StringBuilderPool = new DefaultObjectPool<StringBuilder>(new StringBuilderPooledObjectPolicy());

        public static NoContainer AutoResolver { get; set; }

        internal static class ActivatorPool
        {
            public static readonly Dictionary<Type, CreateInstance> Factory = new Dictionary<Type, CreateInstance>();

            public static T Create<T>() => (T) GetOrBuildActivator<T>()();
            public static object Create(Type type) => GetOrBuildActivator(type);

            private static CreateInstance GetOrBuildActivator<T>() => GetOrBuildActivator(typeof(T));
            private static CreateInstance GetOrBuildActivator(Type type)
            {
                lock (Factory)
                {
                    if (Factory.TryGetValue(type, out var activator))
                        return activator;
                    lock (Factory)
                    {
                        if (!Factory.TryGetValue(type, out activator))
                            Factory.Add(type, activator = Activation.DynamicMethodWeakTyped(type.GetConstructor(Type.EmptyTypes)));
                    }
                    return activator;
                }
            }
        }
        
        /// <summary> The default policy provided by Microsoft uses new T() constraint, which silently defers to Activator.CreateInstance. </summary>
        internal class DefaultObjectPolicy<T> : IPooledObjectPolicy<T>
        {
            internal static T CreateNew() { return ActivatorPool.Create<T>(); }

            public T Create()
            {
                return CreateNew();
            }

            public bool Return(T obj)
            {
                return true;
            }
        }

        internal class ListObjectPolicy<T> : IPooledObjectPolicy<T> where T : IList
        {
            public T Create()
            {
                return DefaultObjectPolicy<T>.CreateNew();
            }

            public bool Return(T obj)
            {
                obj.Clear();
                return obj.Count == 0;
            }
        }

        internal class ActionPoolPolicy : IPooledObjectPolicy<UiAction>
        {
            public UiAction Create()
            {
                return new UiAction();
            }

            public bool Return(UiAction action)
            {
                action.Arguments = null;
                action.MethodName = null;
                return true;
            }
        }
    }
}