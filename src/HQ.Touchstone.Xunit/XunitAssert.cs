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

using System.Collections;
using HQ.Touchstone.Assertions;
using Xunit;
using Xunit.Sdk;

namespace HQ.Touchstone.Xunit
{
    public sealed class XunitAssert : IAssert
    {
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

        private static void TryLogUserMessage(string userMessage, object[] userMessageArgs)
        {
            if (!string.IsNullOrWhiteSpace(userMessage))
                AmbientContext.OutputProvider.WriteLine(userMessage, userMessageArgs);
        }
    }
}
