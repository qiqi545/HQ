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

using System.Collections.Generic;
using HQ.Common;
using HQ.Test.Sdk;

namespace HQ.Platform.Tests.Common
{
    public class FuncEnumerationTests : UnitUnderTest
    {
        [Test]
        public void Can_enumerate()
        {
            var expected = new List<Outer> {new Outer {Value = "A"}, new Outer {Value = "B"}, new Outer {Value = "C"}};
            var actual = new List<string>();

            var enumerable = expected.Enumerate(outer => outer.Value);
            foreach (var value in enumerable)
                actual.Add(value);

            actual.Clear();
            foreach (var value in enumerable)
                actual.Add(value);

            Assert.NotEmpty(actual);
            Assert.Equal(3, actual.Count);
        }

        public class Outer
        {
            public string Value { get; set; }
        }
    }
}
