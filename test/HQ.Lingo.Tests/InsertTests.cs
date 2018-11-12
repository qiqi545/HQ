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
    public class InsertTests : TuxedoTests
    {
        [Test]
        public void Insert_one()
        {
            var query = Tuxedo.Insert(new User {Email = "good@domain.com"});
            Assert.Equal("INSERT INTO User (Email) VALUES (@Email)", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@domain.com", query.Parameters["@Email"]);
            Console.WriteLine(query);
        }
    }
}
