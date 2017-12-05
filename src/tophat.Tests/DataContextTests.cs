using tophat.sqlite;
using Xunit;

namespace tophat.Tests
{
    public class DataContextTests : IClassFixture<DatabaseFixture>
    {
	    private readonly DatabaseFixture _fixture;

	    public DataContextTests(DatabaseFixture fixture)
	    {
		    _fixture = fixture;
	    }

        [Fact]
        public void Data_context_instantiates()
        {
            var cs = _fixture.CreateConnectionString();
            using (var db = new SqliteDataContext(cs))
				Assert.NotNull(db);
		}
    }
}