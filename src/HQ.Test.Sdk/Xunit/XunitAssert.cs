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
using HQ.Test.Sdk.Assertions;
using HQ.Test.Sdk.Logging;
using Xunit;
using Xunit.Sdk;

namespace HQ.Test.Sdk.Xunit
{
    public sealed class XunitAssert : IAssert
    {
        public void Null(object instance, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.Null(instance);
            }
            catch (NullException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void NotNull(object instance, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.NotNull(instance);
            }
            catch (NotNullException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void NotEmpty(IEnumerable enumerable, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.NotEmpty(enumerable);
            }
            catch (NotEmptyException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void True(bool condition, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.True(condition);
            }
            catch (TrueException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void False(bool condition, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.False(condition);
            }
            catch (FalseException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void Equal<T>(T expected, T actual, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.Equal(expected, actual);
            }
            catch (EqualException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void NotEqual<T>(T expected, T actual, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.NotEqual(expected, actual);
            }
            catch (NotEqualException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void Single(IEnumerable collection, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.Single(collection);
            }
            catch (SingleException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void IsType<T>(object instance, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.IsType<T>(instance);
            }
            catch (IsTypeException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void IsType(Type expectedType, object instance, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.IsType(expectedType, instance);
            }
            catch (IsTypeException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void IsNotType(Type expectedType, object instance, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.IsNotType(expectedType, instance);
            }
            catch (IsNotTypeException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void IsNotType<T>(object instance, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.IsNotType<T>(instance);
            }
            catch (IsNotTypeException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        public void Empty(IEnumerable collection, string userMessage = null, params object[] userMessageArgs)
        {
            try
            {
                Assert.Empty(collection);
            }
            catch (NotEmptyException)
            {
                TryLogUserMessage(userMessage, userMessageArgs);
                throw;
            }
        }

        private static void TryLogUserMessage(string userMessage, object[] userMessageArgs)
        {
            if (!string.IsNullOrWhiteSpace(userMessage))
                AmbientContext.OutputProvider.WriteLine(userMessage, userMessageArgs);
        }
    }
}
