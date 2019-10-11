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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Dynamitey;
using Dynamitey.DynamicObjects;
using HQ.Test.Sdk.Internal;
using HQ.Test.Sdk.Logging;
using ImpromptuInterface;
using Microsoft.Extensions.Logging;
using TestKitchen;
using TypeKitchen;

namespace HQ.Test.Sdk
{
    public abstract class TestScope : LoggingScope
    {
        protected readonly ILoggerFactory DefaultLoggerFactory = new LoggerFactory();
        public IServiceProvider ServiceProvider;

        protected static DelegateLoggerProvider CreateLoggerProvider()
        {
            var actionLoggerProvider = new DelegateLoggerProvider(message =>
            {
                var outputProvider = AmbientContext.OutputProvider;
                if (outputProvider?.IsAvailable != true)
                    return;
                outputProvider.WriteLine(message);
            });
            return actionLoggerProvider;
        }
		
        #region Objects API

        protected TInterface Dummy<TInterface>()
        {
            return new Dummy().ActLike(typeof(TInterface));
        }

        protected TInterface Stub<TInterface>(object prototype)
            where TInterface : class
        {
            return prototype.ActLike<TInterface>();
        }

        protected TInterface Stub<TInterface, TPrototype>()
            where TInterface : class
            where TPrototype : class, TInterface
        {
            return TryGetImplementation<TInterface, TPrototype>().ActLike(typeof(TInterface));
        }

        protected TInterface Stub<TInterface, TPrototype>(TPrototype prototype)
            where TInterface : class
            where TPrototype : TInterface
        {
            return prototype.ActLike<TInterface>();
        }

        protected TInterface Stub<TInterface, TPrototype>(object prototype)
            where TInterface : class
            where TPrototype : class, TInterface
        {
            var wrapper =
                new Prototype<TInterface>(TryGetImplementation<TInterface, TPrototype>().ActLike<TInterface>(),
                    prototype);
            return wrapper.ActLike(typeof(TInterface));
        }

        protected TInterface Stub<TInterface, TPrototype>(TInterface implementation, object prototype)
            where TInterface : class
            where TPrototype : class, TInterface
        {
            var wrapper = new Prototype<TInterface>(
                implementation ?? TryGetImplementation<TInterface, TPrototype>().ActLike(typeof(TInterface)),
                prototype);
            return wrapper.ActLike(typeof(TInterface));
        }

        private TPrototype TryGetImplementation<TInterface, TPrototype>()
            where TPrototype : class, TInterface
        {
            return ServiceProvider?.GetService(typeof(TPrototype)) as TPrototype ??
                   Instancing.CreateInstance<TPrototype>() ??
                   Dummy<TPrototype>();
        }

        private class Prototype<TInterface> : DynamicObject
        {
            private readonly object _child;
            private readonly ImmutableHashSet<string> _memberNames;
            private readonly TInterface _parent;

            public Prototype(TInterface parent, object child)
            {
                _parent = parent;
                _child = child;
                _memberNames =
                    AccessorMembers.Create(typeof(TInterface)).Select(x => x.Name)
                        .Concat(typeof(TInterface).GetMethods().Select(x => x.Name))
                        .ToImmutableHashSet();
            }

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                if (!_memberNames.Contains(binder.Name))
                {
                    result = default;
                    return false;
                }

                result = !HasMember(_child, binder.Name, binder.ReturnType)
                    ? Dynamic.InvokeGet(_parent, binder.Name)
                    : Dynamic.InvokeGet(_child, binder.Name) ?? Dynamic.InvokeGet(_parent, binder.Name);

                return result != default;
            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                if (!_memberNames.Contains(binder.Name))
                    return false;

                if (!HasMember(_child, binder.Name, binder.ReturnType))
                    return Dynamic.InvokeSet(_parent, binder.Name, value) != default;

                return Dynamic.InvokeSet(_child, binder.Name, value) == default &&
                       Dynamic.InvokeSet(_parent, binder.Name, value) != default;
            }

            public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
            {
                if (!_memberNames.Contains(binder.Name))
                {
                    result = default;
                    return false;
                }

                if (!HasMember(_child, binder.Name, binder.ReturnType))
                    result = Dynamic.InvokeMember(_parent, binder.Name, args);
                else
                    result = Dynamic.InvokeMember(_child, binder.Name, args) ??
                             Dynamic.InvokeMember(_parent, binder.Name, args);

                return result != default;
            }

            public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
            {
                result = Dynamic.InvokeGetIndex(_child, indexes) ??
                         Dynamic.InvokeGetIndex(_parent, indexes);

                return result != default;
            }

            public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
            {
                if (Dynamic.InvokeSetIndex(_child, indexes, value) == default)
                    return Dynamic.InvokeSetIndex(_parent, indexes, value) != default;
                return false;
            }

            private static bool HasMember(dynamic instance, string name, Type type)
            {
                if (instance == null)
                    return false;
                if (instance is IDictionary<string, object> dictionary)
                    return dictionary.TryGetValue(name, out var member) && member.GetType() == type;

                Type instanceType = instance.GetType();
                if (instanceType.GetProperty(name) is PropertyInfo _)
                    return true;
                if (instanceType.GetField(name) is FieldInfo _)
                    return true;
                if (instanceType.GetMethod(name) is MethodInfo _)
                    return true;

                return false;
            }
        }

        #endregion
    }
}



