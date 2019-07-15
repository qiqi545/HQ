using System;
using HQ.Integration.DocumentDb.DbProvider;
using Xunit;

namespace HQ.Integration.Tests
{
	public class DocumentDbConnectionStringBuilderTests
	{
		[Fact]
		public void Can_access_case_insensitive()
		{
			var builder = new DocumentDbConnectionStringBuilder
			{
				AccountEndpoint = new Uri("https://example.documents.azure.com:443/", UriKind.Absolute),
				AccountKey = "MyAccountKey",
				Database = "MyDatabase"
			};

			Assert.NotNull(builder["AccountKey"]);
			Assert.NotNull(builder["accountkey"]);
			Assert.NotNull(builder.AccountKey);

			builder.Build();
		}

		[Fact]
		public void Can_build_connection_string()
		{
			const string expected =
				"AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=MyAccountKey;Database=MyDatabase";

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

		[Fact]
		public void Can_parse_connection_string()
		{
			const string expected =
				"AccountEndpoint=https://example.documents.azure.com:443/;AccountKey=MyAccountKey;Database=MyDatabase";
			var builder = new DocumentDbConnectionStringBuilder(expected);
			Assert.Equal(expected, builder.ConnectionString, StringComparer.OrdinalIgnoreCase);

			builder.AccountKey = "MyOtherAccountKey";
			Assert.NotEqual(expected, builder.ConnectionString, StringComparer.OrdinalIgnoreCase);
		}
	}
}