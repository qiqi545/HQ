using System.Data;
using FluentMigrator;

namespace HQ.Cohort.Stores.Sql.MySql
{
	public enum SupportedDatabases
	{
		SqlServer,
		Sqlite,
		MySql
	}

	public class ZeroMigrationContext
	{
		public SupportedDatabases Database { get; set; }
	}

	[Migration(0)]
	public class CreateIdentitySchema : Migration
    {
	    private readonly ZeroMigrationContext _context;

	    public CreateIdentitySchema(ZeroMigrationContext context)
	    {
		    _context = context;
	    }

	    public override void Up()
	    {
			/*
		      migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });
			*/
		    Create.Table("AspNetRoles")
			    .WithColumn("Id").AsString(450).NotNullable().PrimaryKey("PK_AspNetRoles")
			    .WithColumn("Name").AsString(256).Nullable()
			    .WithColumn("NormalizedName").AsString(256).Nullable()
			    .WithColumn("ConcurrencyStamp").AsString(int.MaxValue).Nullable()
			    ;

			/*
            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });
			*/
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

			/*
            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
			*/
		    Create.Table("AspNetRoleClaims")
			    .WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey("PK_AspNetRoleClaims")
			    .WithColumn("RoleId").AsString(450).NotNullable().ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", "AspNetRoles", "Id").OnDelete(Rule.Cascade)
			    .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
			    .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable()
			    ;

		   /*
		   migrationBuilder.CreateTable(
			   name: "AspNetUserClaims",
			   columns: table => new
			   {
				   Id = table.Column<int>(nullable: false)
					   .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
				   UserId = table.Column<string>(nullable: false),
				   ClaimType = table.Column<string>(nullable: true),
				   ClaimValue = table.Column<string>(nullable: true)
			   },
			   constraints: table =>
			   {
				   table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
				   table.ForeignKey(
					   name: "FK_AspNetUserClaims_AspNetUsers_UserId",
					   column: x => x.UserId,
					   principalTable: "AspNetUsers",
					   principalColumn: "Id",
					   onDelete: ReferentialAction.Cascade);
			   });
		   */
		   Create.Table("AspNetUserClaims")
			    .WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey("PK_AspNetUserClaims")
			    .WithColumn("UserId").AsString(450).NotNullable().ForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
			    .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
			    .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable()
			    ;

			/*
            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
			*/
		    Create.Table("AspNetUserLogins")
			    .WithColumn("LoginProvider").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserLogins")
			    .WithColumn("ProviderKey").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserLogins")
			    .WithColumn("ProviderDisplayName").AsString(int.MaxValue).Nullable()
			    .WithColumn("UserId").AsString(450).NotNullable().ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
			    ;

			/*
            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
			*/
		    Create.Table("AspNetUserRoles")
			    .WithColumn("UserId").AsString(450).NotNullable()
				    .PrimaryKey("PK_AspNetUserRoles")
				    .ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", "AspNetRoles", "Id").OnDelete(Rule.Cascade)
			    .WithColumn("RoleId").AsString(450).NotNullable()
				    .PrimaryKey("PK_AspNetUserRoles")
				    .ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
			    ;

			/*
            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
			*/
		    Create.Table("AspNetUserTokens")
			    .WithColumn("UserId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUserTokens").ForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
			    .WithColumn("LoginProvider").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserTokens")
			    .WithColumn("Name").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserTokens")
			    .WithColumn("Value").AsString(int.MaxValue).Nullable()
				;

			/*
            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");
		    */
			Create.Index("IX_AspNetRoleClaims_RoleId").OnTable("AspNetRoleClaims").OnColumn("RoleId");

			/*
            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");
			*/
			switch (_context.Database)
	        {
		        case SupportedDatabases.SqlServer:
			        Execute.Sql(@"
CREATE UNIQUE INDEX [RoleNameIndex]
    ON [dbo].[AspNetRoles]([NormalizedName])
WHERE [NormalizedName] IS NOT NULL;");
				    break;
		        default:
				    Create.Index("RoleNameIndex").OnTable("AspNetRoles").OnColumn("NormalizedName").Unique();
			        break;
		    }

			/*
			migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");
			*/
		    Create.Index("IX_AspNetUserClaims_UserId").OnTable("AspNetUserClaims").OnColumn("UserId");

			/*
		    migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");
			*/
		    Create.Index("IX_AspNetUserLogins_UserId").OnTable("AspNetUserLogins").OnColumn("UserId");

			/*
		    migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");
			*/
		    Create.Index("IX_AspNetUserRoles_RoleId").OnTable("AspNetUserRoles").OnColumn("RoleId");

			/*
            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");
			*/
		    Create.Index("EmailIndex").OnTable("AspNetUsers").OnColumn("NormalizedEmail");

			/*
            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
             */
		    switch (_context.Database)
		    {
			    case SupportedDatabases.SqlServer:
				    Execute.Sql(@"
CREATE UNIQUE INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([NormalizedUserName])
WHERE [NormalizedUserName] IS NOT NULL;");
				    break;
			    default:
				    Create.Index("UserNameIndex").OnTable("AspNetUsers").OnColumn("NormalizedUserName").Unique();
				    break;
		    }
		}

		public override void Down()
		{
			Delete.Table("AspNetRoleClaims");
			Delete.Table("AspNetUserClaims");
			Delete.Table("AspNetUserLogins");
			Delete.Table("AspNetUserRoles");
			Delete.Table("AspNetUserTokens");
			Delete.Table("AspNetRoles");
			Delete.Table("AspNetUsers");
		}
    }
}
