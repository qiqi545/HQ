using Xunit;

namespace System.Data.DocumentDb.Tests
{
    public class DocumentDbConnectionStringBuilderTests
    {
        [Fact]
        public void Can_parse_connection_string()
        {
            const string expected = "AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=MyAccountKey;Database=MyDatabase";
            var builder = new DocumentDbConnectionStringBuilder(expected);
            Assert.Equal(expected, builder.ConnectionString, StringComparer.OrdinalIgnoreCase);

            builder.AccountKey = "MyOtherAccountKey";
            Assert.NotEqual(expected, builder.ConnectionString, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void Can_build_connection_string()
        {
            var expected = "AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=MyAccountKey;Database=MyDatabase";

            var builder = new DocumentDbConnectionStringBuilder
            {
                AccountEndpoint = new Uri("https://example.documents.azure.com:443/", UriKind.Absolute),
                AccountKey = "MyAccountKey",
                Database = "MyDatabase"
            };

            Assert.Equal(expected, builder.ConnectionString, StringComparer.OrdinalIgnoreCase);

            builder.AccountKey = "MyOtherAccountKey";
            Assert.NotEqual(expected, builder.ConnectionString, StringComparer.OrdinalIgnoreCase);
        }
    }
}
