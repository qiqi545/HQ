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

namespace HQ.Touchstone.Assertions
{
    public interface IAssert
    {
        void Null(object instance, string userMessage = null, params object[] userMessageArgs);
        void NotNull(object instance, string userMessage = null, params object[] userMessageArgs);

        void Empty(IEnumerable collection, string userMessage = null, params object[] userMessageArgs);
        void NotEmpty(IEnumerable enumerable, string userMessage = null, params object[] userMessageArgs);

        void True(bool condition, string userMessage = null, params object[] userMessageArgs);
        void False(bool condition, string userMessage = null, params object[] userMessageArgs);

        void Equal<T>(T expected, T actual, string userMessage = null, params object[] userMessageArgs);
        void NotEqual<T>(T expected, T actual, string userMessage = null, params object[] userMessageArgs);

        void Single(IEnumerable collection, string userMessage = null, params object[] userMessageArgs);

        void IsType<T>(object instance, string userMessage = null, params object[] userMessageArgs);
        void IsType(Type expectedType, object instance, string userMessage = null, params object[] userMessageArgs);
        void IsNotType<T>(object instance, string userMessage = null, params object[] userMessageArgs);
        void IsNotType(Type expectedType, object instance, string userMessage = null, params object[] userMessageArgs);
    }
}
