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

using System.Text;

namespace HQ.Platform.Tests
{
    namespace V1
    {
        public class Person
        {
            public string Name { get; set; }

            public int BufferSize => 1 + sizeof(int) + Encoding.UTF8.GetByteCount(Name);
        }
    }

    namespace V2
    {
        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public int BufferSize =>
                1 + sizeof(int) + Encoding.UTF8.GetByteCount(FirstName) +
                1 + sizeof(int) + Encoding.UTF8.GetByteCount(LastName);
        }
    }
}
