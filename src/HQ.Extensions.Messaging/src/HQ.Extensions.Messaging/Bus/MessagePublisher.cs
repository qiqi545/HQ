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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace HQ.Extensions.Messaging.Bus
{
    public sealed class MessagePublisher : IMessagePublisher
    {
        private readonly IMessageAggregator _aggregator;
        private readonly Hashtable _byTypeDispatch = new Hashtable();

        public MessagePublisher(IMessageAggregator aggregator)
        {
            _aggregator = aggregator;
        }

        public bool Publish(object message)
        {
            var type = message.GetType();
            if (!(_byTypeDispatch[type] is Func<object, bool> dispatcher))
            {
                dispatcher = BuildByTypeDispatcher(type);
                _byTypeDispatch[type] = dispatcher;
            }

            return dispatcher(message);
        }

        public IEnumerable<Type> GetAncestors(Type type)
        {
            foreach (var i in type?.GetInterfaces() ?? Type.EmptyTypes)
            {
                yield return i;
            }

            if (type?.BaseType == null || type.BaseType == typeof(object))
            {
                yield break;
            }

            var baseType = type.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }

        private Func<object, bool> BuildByTypeDispatcher(Type superType)
        {
            const BindingFlags binding = BindingFlags.NonPublic | BindingFlags.Instance;
            var publishTyped = typeof(MessagePublisher).GetMethod(nameof(PublishTyped), binding);
            Debug.Assert(publishTyped != null);

            var dispatchers = new Dictionary<Type, MethodInfo>
            {
                {superType, publishTyped.MakeGenericMethod(superType)}
            };

            foreach (var childType in GetAncestors(superType))
            {
                dispatchers.Add(childType, publishTyped.MakeGenericMethod(childType));
            }

            bool Dispatch(object message)
            {
                var result = true;
                foreach (var dispatcher in dispatchers)
                {
                    var method = dispatcher.Value;
                    var handled = (bool) method.Invoke(this, new[] {message});
                    result &= handled;
                }

                return result;
            }

            return Dispatch;
        }

        private bool PublishTyped<T>(T message)
        {
            return _aggregator.Handle(typeof(T), message);
        }
    }
}
