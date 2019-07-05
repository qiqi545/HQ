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

using Xunit.Sdk;

namespace HQ.Test.Sdk.Xunit
{
    public sealed class XunitOutputProvider : IOutputProvider
    {
        internal TestOutputHelper Inner;

        public XunitOutputProvider()
        {
            Inner = new TestOutputHelper();
        }

        public bool IsAvailable => Inner != null;

        public void WriteLine(string message)
        {
            Inner.WriteLine(message);
        }

        public void WriteLine(string format, params object[] args)
        {
            Inner.WriteLine(format, args);
        }
    }
}
