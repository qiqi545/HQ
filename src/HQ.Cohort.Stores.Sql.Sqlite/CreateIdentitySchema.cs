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

using System.Data;
using FluentMigrator;

namespace HQ.Cohort.Stores.Sql.Sqlite
{
    [Migration(0)]
    public class CreateIdentitySchema : Migration
    {
        public override void Up()
        {
            Create.Table("AspNetRoles")
                .WithColumn("Id").AsString(450).NotNullable().PrimaryKey("PK_AspNetRoles")
                .WithColumn("Name").AsString(256).Nullable()
                .WithColumn("NormalizedName").AsString(256).Nullable()
                .WithColumn("ConcurrencyStamp").AsString(int.MaxValue).Nullable()
                ;

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
                .WithColumn("LockoutEnd").AsDateTime().Nullable()
                .WithColumn("LockoutEnabled").AsBoolean().NotNullable()
                .WithColumn("AccessFailedCount").AsInt32().NotNullable()
                ;

            Create.Table("AspNetRoleClaims")
                .WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey("PK_AspNetRoleClaims")
                .WithColumn("RoleId").AsString(450).NotNullable()
                .ForeignKey("FK_AspNetRoleClaims_AspNetRoles_RoleId", "AspNetRoles", "Id").OnDelete(Rule.Cascade)
                .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
                .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable()
                ;

            Create.Table("AspNetUserClaims")
                .WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey("PK_AspNetUserClaims")
                .WithColumn("UserId").AsString(450).NotNullable()
                .ForeignKey("FK_AspNetUserClaims_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
                .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
                .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable()
                ;

            Create.Table("AspNetUserLogins")
                .WithColumn("LoginProvider").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserLogins")
                .WithColumn("ProviderKey").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserLogins")
                .WithColumn("ProviderDisplayName").AsString(int.MaxValue).Nullable()
                .WithColumn("UserId").AsString(450).NotNullable()
                .ForeignKey("FK_AspNetUserLogins_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
                ;

            Create.Table("AspNetUserRoles")
                .WithColumn("UserId").AsString(450).NotNullable()
                .PrimaryKey("PK_AspNetUserRoles")
                .ForeignKey("FK_AspNetUserRoles_AspNetRoles_RoleId", "AspNetRoles", "Id").OnDelete(Rule.Cascade)
                .WithColumn("RoleId").AsString(450).NotNullable()
                .PrimaryKey("PK_AspNetUserRoles")
                .ForeignKey("FK_AspNetUserRoles_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
                ;

            Create.Table("AspNetUserTokens")
                .WithColumn("UserId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUserTokens")
                .ForeignKey("FK_AspNetUserTokens_AspNetUsers_UserId", "AspNetUsers", "Id").OnDelete(Rule.Cascade)
                .WithColumn("LoginProvider").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserTokens")
                .WithColumn("Name").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserTokens")
                .WithColumn("Value").AsString(int.MaxValue).Nullable()
                ;

            Create.Index("IX_AspNetRoleClaims_RoleId").OnTable("AspNetRoleClaims").OnColumn("RoleId");
            Create.Index("RoleNameIndex").OnTable("AspNetRoles").OnColumn("NormalizedName").Unique();
            Create.Index("IX_AspNetUserClaims_UserId").OnTable("AspNetUserClaims").OnColumn("UserId");
            Create.Index("IX_AspNetUserLogins_UserId").OnTable("AspNetUserLogins").OnColumn("UserId");
            Create.Index("IX_AspNetUserRoles_RoleId").OnTable("AspNetUserRoles").OnColumn("RoleId");
            Create.Index("EmailIndex").OnTable("AspNetUsers").OnColumn("NormalizedEmail");
            Create.Index("UserNameIndex").OnTable("AspNetUsers").OnColumn("NormalizedUserName").Unique();
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
