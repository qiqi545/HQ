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
using System.Linq;
using HQ.Common;
using HQ.Test.Sdk;
using Xunit;

namespace HQ.Platform.Tests.Common
{
    public class EnumerableExtensionsTests : UnitUnderTest
    {
        [Fact]
        public void Enumerable_is_already_a_list()
        {
            IEnumerable<string> enumerable = new List<string>();
            var list = enumerable.MaybeList();
            Assert.Equal(enumerable, list);
            Assert.StrictEqual(enumerable, list);
        }

        [Test]
        public void Enumerable_is_not_a_list()
        {
            var enumerable = Enumerable.Repeat(1, 10);

            // ReSharper disable once PossibleMultipleEnumeration
            var list = enumerable.MaybeList();

            // ReSharper disable once PossibleMultipleEnumeration
            Assert.Equal(enumerable, list);

            // ReSharper disable once PossibleMultipleEnumeration
            Assert.NotStrictEqual(enumerable, list);
        }
    }
}
