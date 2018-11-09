// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace HQ.Flow.Bus
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
                yield return i;

            if (type?.BaseType == null || type.BaseType == typeof(object))
                yield break;

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
                dispatchers.Add(childType, publishTyped.MakeGenericMethod(childType));

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
