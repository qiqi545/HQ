using System;

namespace tophat.Tests
{
    public class DatabaseFixture : IDisposable
    {
        public string CreateConnectionString()
        {
	        return $"Data Source={Guid.NewGuid()}.sqdb;Mode=ReadWriteCreate;";
        }

		public void Dispose() { }
    }
}