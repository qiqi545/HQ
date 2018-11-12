using System;
using NUnit.Framework;
using tuxedo.Tests.Models;

namespace tuxedo.Tests
{
    [TestFixture]
    public class InsertTests : TuxedoTests
    {
        [Test]
        public void Insert_one()
        {
            var query = Tuxedo.Insert(new User { Email = "good@domain.com"});
            Assert.Equal("INSERT INTO User (Email) VALUES (@Email)", query.Sql);
            Assert.Equal(1, query.Parameters.Count);
            Assert.Equal("good@domain.com", query.Parameters["@Email"]);
            Console.WriteLine(query);
        }
    }
}
