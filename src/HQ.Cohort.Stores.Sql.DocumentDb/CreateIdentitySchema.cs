using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace HQ.Cohort.Stores.Sql.DocumentDb
{
	public class CreateIdentitySchema
    {
	    private readonly DocumentClient _client;
	    private readonly string _databaseId;

	    public CreateIdentitySchema(DocumentClient client, string databaseId)
	    {
		    _client = client;
		    _databaseId = databaseId;
	    }

	    public async Task Up()
	    {
		    await CreateCollectionIfNotExistsAsync("AspNetIdentity");

			/*
			Create.Table("AspNetRoles")
			    .WithColumn("Id").AsString(450).NotNullable().PrimaryKey("PK_AspNetRoles")
			    .WithColumn("Name").AsString(256).Nullable()
			    .WithColumn("NormalizedName").AsString(256).Nullable()
			    .WithColumn("ConcurrencyStamp").AsString(int.MaxValue).Nullable()
			    ;
			*/

			/*
			Create.Table("AspNetUsers")
			    .WithColumn("Id").AsString(450).NotNullable().PrimaryKey("PK_AspNetUsers")
			    .WithColumn("UserName").AsString(256).Nullable()
			    .WithColumn("NormalizedUserName").AsString(256).Nullable()
			    .WithColumn("Email").AsString(256).Nullable()
			    .WithColumn("NormalizedEmail").AsString(256).Nullable()
			    .WithColumn("EmailConfirmed").AsBoolean().NotNullable()
			    .WithColumn("PasswordHash").AsString(int.MaxValue).Nullable()
			    .WithColumn("SecurityStamp").AsString(int.MaxValue).Nullable()
			    .WithColumn("ConcurrencyStamp").AsString(int.MaxValue).Nullable()
			    .WithColumn("PhoneNumber").AsString(int.MaxValue).Nullable()
			    .WithColumn("PhoneNumberConfirmed").AsBoolean().NotNullable()
			    .WithColumn("TwoFactorEnabled").AsBoolean().NotNullable()
			    .WithColumn("LockoutEnd").AsDateTimeOffset().Nullable()
			    .WithColumn("LockoutEnabled").AsBoolean().NotNullable()
			    .WithColumn("AccessFailedCount").AsInt32().NotNullable()
				;
			*/

			/*
			Create.Table("AspNetRoleClaims")
			    .WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey("PK_AspNetRoleClaims")
			    .WithColumn("RoleId").AsString(450).NotNullable().ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", "AspNetRoles", "Id").OnDelete(Rule.Cascade)
			    .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
			    .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable()
			    ;
			*/

			/*
			Create.Table("AspNetUserClaims")
			    .WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey("PK_AspNetUserClaims")
			    .WithColumn("UserId").AsString(450).NotNullable().ForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
			    .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
			    .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable()
			    ;
			*/

			/*
			Create.Table("AspNetUserLogins")
			    .WithColumn("LoginProvider").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserLogins")
			    .WithColumn("ProviderKey").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserLogins")
			    .WithColumn("ProviderDisplayName").AsString(int.MaxValue).Nullable()
			    .WithColumn("UserId").AsString(450).NotNullable().ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
			    ;
			*/

			/*
			Create.Table("AspNetUserRoles")
			    .WithColumn("UserId").AsString(450).NotNullable()
				    .PrimaryKey("PK_AspNetUserRoles")
				    .ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", "AspNetRoles", "Id").OnDelete(Rule.Cascade)
			    .WithColumn("RoleId").AsString(450).NotNullable()
				    .PrimaryKey("PK_AspNetUserRoles")
				    .ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
			    ;
			*/

			/*
			Create.Table("AspNetUserTokens")
			    .WithColumn("UserId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUserTokens").ForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
			    .WithColumn("LoginProvider").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserTokens")
			    .WithColumn("Name").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserTokens")
			    .WithColumn("Value").AsString(int.MaxValue).Nullable()
				;
			*/

			/*
			Create.Index("IX_AspNetRoleClaims_RoleId").OnTable("AspNetRoleClaims").OnColumn("RoleId");
		    Create.Index("RoleNameIndex").OnTable("AspNetRoles").OnColumn("NormalizedName").Unique();
			Create.Index("IX_AspNetUserClaims_UserId").OnTable("AspNetUserClaims").OnColumn("UserId");
			Create.Index("IX_AspNetUserLogins_UserId").OnTable("AspNetUserLogins").OnColumn("UserId");
			Create.Index("IX_AspNetUserRoles_RoleId").OnTable("AspNetUserRoles").OnColumn("RoleId");
			Create.Index("EmailIndex").OnTable("AspNetUsers").OnColumn("NormalizedEmail");
		    Create.Index("UserNameIndex").OnTable("AspNetUsers").OnColumn("NormalizedUserName").Unique();
			*/
		}
		
	    private async Task CreateCollectionIfNotExistsAsync(string collectionId)
	    {
		    try
		    {
			    await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_databaseId, collectionId));
		    }
		    catch (DocumentClientException e)
		    {
			    if (e.StatusCode == HttpStatusCode.NotFound)
			    {
				    await _client.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(_databaseId), new DocumentCollection { Id = collectionId }, new RequestOptions { OfferThroughput = 400 });
			    }
			    else
			    {
				    throw;
			    }
		    }
	    }
	}
}
