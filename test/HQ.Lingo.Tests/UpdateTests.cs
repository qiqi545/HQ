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
using HQ.Lingo.Tests.Models;
using HQ.Touchstone.Xunit;
using Xunit;

namespace HQ.Lingo.Tests
{
    public class UpdateTests : TuxedoTests
    {
        [Test]
        public void Update_with_no_where()
        {
            var query = Tuxedo.Update<User>(new {Email = "good@domain.com"});
            Assert.Equal("UPDATE User SET Email = @Email_set", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@domain.com", query.Parameters["@Email_set"]);
            Console.WriteLine(query);
        }

        [Test]
        public void Update_with_explicit_where()
        {
            var query = Tuxedo.Update<User>(new {Email = "good@domain.com"}, new {Email = "bad@domain.com"});
            Assert.Equal("UPDATE User SET Email = @Email_set WHERE Email = @Email", query.Sql);
            Assert.Equal(2, query.Parameters.Count);
            Assert.Equal("bad@domain.com", query.Parameters["@Email"]);
            Assert.Equal("good@domain.com", query.Parameters["@Email_set"]);
            Console.WriteLine(query);
        }
    }
}
