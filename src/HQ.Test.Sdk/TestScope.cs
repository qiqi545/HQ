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
using HQ.Extensions.CodeGeneration;
using HQ.Extensions.Logging;
using ImpromptuInterface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using TypeKitchen;

namespace HQ.Test.Sdk
{
    public abstract class TestScope : ILogger
    {
        protected readonly ILoggerFactory defaultLoggerFactory = new LoggerFactory();
        protected IServiceProvider serviceProvider;

        protected static ActionLoggerProvider CreateLoggerProvider()
        {
            var actionLoggerProvider = new ActionLoggerProvider(message =>
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
            return serviceProvider?.GetService(typeof(TPrototype)) as TPrototype ??
                   InstanceFactory.Default.CreateInstance<TPrototype>() ??
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

        #region Logging

        protected static string MessageFormatter(object state, Exception error)
        {
            return state.ToString();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            var logger = GetLogger();
            logger?.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            var logger = GetLogger();
            return logger != null && logger.IsEnabled(logLevel);
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            var logger = GetLogger();
            return logger?.BeginScope(state);
        }

        public abstract ILogger GetLogger();

        #region Logging Forwards (see Microsoft.Extensions.LoggerExtensions.cs)

        public void LogDebug(EventId eventId, Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Debug, eventId, exception, message, args);
        }

        public void LogDebug(EventId eventId, string message, params object[] args)
        {
            Log(LogLevel.Debug, eventId, message, args);
        }

        public void LogDebug(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Debug, exception, message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            Log(LogLevel.Debug, message, args);
        }

        public void LogTrace(EventId eventId, Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Trace, eventId, exception, message, args);
        }

        public void LogTrace(EventId eventId, string message, params object[] args)
        {
            Log(LogLevel.Trace, eventId, message, args);
        }

        public void LogTrace(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Trace, exception, message, args);
        }

        public void LogTrace(string message, params object[] args)
        {
            Log(LogLevel.Trace, message, args);
        }

        public void LogInformation(EventId eventId, Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Information, eventId, exception, message, args);
        }

        public void LogInformation(EventId eventId, string message, params object[] args)
        {
            Log(LogLevel.Information, eventId, message, args);
        }

        public void LogInformation(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Information, exception, message, args);
        }

        public void LogInformation(string message, params object[] args)
        {
            Log(LogLevel.Information, message, args);
        }

        public void LogWarning(EventId eventId, Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Warning, eventId, exception, message, args);
        }

        public void LogWarning(EventId eventId, string message, params object[] args)
        {
            Log(LogLevel.Warning, eventId, message, args);
        }

        public void LogWarning(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Warning, exception, message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            Log(LogLevel.Warning, message, args);
        }

        public void LogError(EventId eventId, Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Error, eventId, exception, message, args);
        }

        public void LogError(EventId eventId, string message, params object[] args)
        {
            Log(LogLevel.Error, eventId, message, args);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Error, exception, message, args);
        }

        public void LogError(string message, params object[] args)
        {
            Log(LogLevel.Error, message, args);
        }

        public void LogCritical(EventId eventId, Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Critical, eventId, exception, message, args);
        }

        public void LogCritical(EventId eventId, string message, params object[] args)
        {
            Log(LogLevel.Critical, eventId, message, args);
        }

        public void LogCritical(Exception exception, string message, params object[] args)
        {
            Log(LogLevel.Critical, exception, message, args);
        }

        public void LogCritical(string message, params object[] args)
        {
            Log(LogLevel.Critical, message, args);
        }

        public void Log(LogLevel logLevel, string message, params object[] args)
        {
            Log(logLevel, 0, null, message, args);
        }

        public void Log(LogLevel logLevel, EventId eventId, string message, params object[] args)
        {
            Log(logLevel, eventId, null, message, args);
        }

        public void Log(LogLevel logLevel, Exception exception, string message, params object[] args)
        {
            Log(logLevel, 0, exception, message, args);
        }

        public void Log(LogLevel logLevel, EventId eventId, Exception exception, string message, params object[] args)
        {
            Log(logLevel, eventId, (object) new FormattedLogValues(message, args), exception, MessageFormatter);
        }

        public IDisposable BeginScope(string messageFormat, params object[] args)
        {
            return BeginScope(new FormattedLogValues(messageFormat, args));
        }

        #endregion

        #endregion
    }
}
