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

namespace HQ.Integration.Sqlite.Identity
{
    [Migration(0)]
    public class CreateIdentitySchema : Migration
    {
        public override void Up()
        {
            Create.Table("AspNetTenants")
                .WithColumn("Id").AsString(450).NotNullable().PrimaryKey("PK_AspNetTenants")
                .WithColumn("Name").AsString(256).Nullable()
                .WithColumn("NormalizedName").AsString(256).Nullable()
                .WithColumn("SecurityStamp").AsString(int.MaxValue).Nullable()
                .WithColumn("ConcurrencyStamp").AsString(int.MaxValue).Nullable()
                ;

            Create.Table("AspNetApplications")
                .WithColumn("Id").AsString(450).NotNullable().PrimaryKey("PK_AspNetApplications")
                .WithColumn("Name").AsString(256).Nullable()
                .WithColumn("NormalizedName").AsString(256).Nullable()
                .WithColumn("SecurityStamp").AsString(int.MaxValue).Nullable()
                .WithColumn("ConcurrencyStamp").AsString(int.MaxValue).Nullable()
                ;

            Create.Table("AspNetRoles")
                .WithColumn("ApplicationId").AsString(450).NotNullable().PrimaryKey("PK_AspNetRoles")
                .WithColumn("Id").AsString(450).NotNullable().PrimaryKey("PK_AspNetRoles")
                .WithColumn("Name").AsString(256).Nullable()
                .WithColumn("NormalizedName").AsString(256).Nullable()
                .WithColumn("ConcurrencyStamp").AsString(int.MaxValue).Nullable()
                ;

            Create.Table("AspNetUsers")
                .WithColumn("TenantId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUsers")
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

            Create.Table("AspNetApplicationRoles")
                .WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey("PK_AspNetApplicationRoles")
                .WithColumn("ApplicationId").AsString(450).NotNullable()
                .WithColumn("RoleId").AsString(450).NotNullable()
                ;
            Create.ForeignKey("FK_AspNetApplicationRoles_AspNetApplications")
                .FromTable("AspNetApplicationRoles").ForeignColumns("ApplicationId")
                .ToTable("AspNetApplications").PrimaryColumns("Id")
                .OnDelete(Rule.Cascade)
                ;
            Create.ForeignKey("FK_AspNetApplicationRoles_AspNetRoles")
                .FromTable("AspNetApplicationRoles").ForeignColumns("ApplicationId", "RoleId")
                .ToTable("AspNetRole").PrimaryColumns("ApplicationId", "Id")
                .OnDelete(Rule.Cascade)
                ;

            Create.Table("AspNetRoleClaims")
                .WithColumn("ApplicationId").AsString(450).NotNullable().PrimaryKey("PK_AspNetRoleClaims")
                .WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey("PK_AspNetRoleClaims")
                .WithColumn("RoleId").AsString(450).NotNullable()
                .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
                .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable()
                ;
            Create.ForeignKey("FK_AspNetRoleClaims_AspNetRoles")
                .FromTable("AspNetRoleClaims").ForeignColumns("ApplicationId", "RoleId")
                .ToTable("AspNetRole").PrimaryColumns("ApplicationId", "Id")
                .OnDelete(Rule.Cascade)
                ;

            Create.Table("AspNetUserClaims")
                .WithColumn("TenantId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUserClaims")
                .WithColumn("Id").AsInt32().Identity().NotNullable().PrimaryKey("PK_AspNetUserClaims")
                .WithColumn("UserId").AsString(450).NotNullable()
                .WithColumn("ClaimType").AsString(int.MaxValue).Nullable()
                .WithColumn("ClaimValue").AsString(int.MaxValue).Nullable()
                ;
            Create.ForeignKey("FK_AspNetUserClaims_AspNetUsers")
                .FromTable("AspNetUserClaims").ForeignColumns("TenantId", "UserId")
                .ToTable("AspNetUsers").PrimaryColumns("TenantId", "Id")
                .OnDelete(Rule.Cascade)
                ;

            Create.Table("AspNetUserLogins")
                .WithColumn("TenantId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUserLogins")
                .WithColumn("LoginProvider").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserLogins")
                .WithColumn("ProviderKey").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserLogins")
                .WithColumn("ProviderDisplayName").AsString(int.MaxValue).Nullable()
                .WithColumn("UserId").AsString(450).NotNullable()
                ;
            Create.ForeignKey("FK_AspNetUserLogins_AspNetUsers")
                .FromTable("AspNetUserLogins").ForeignColumns("TenantId", "UserId")
                .ToTable("AspNetUsers").PrimaryColumns("TenantId", "Id")
                .OnDelete(Rule.Cascade)
                ;

            Create.Table("AspNetUserRoles")
                .WithColumn("TenantId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUserRoles")
                .WithColumn("ApplicationId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUserRoles")
                .WithColumn("UserId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUserRoles")
                .WithColumn("RoleId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUserRoles")
                ;
            Create.ForeignKey("FK_AspNetUserRoles_AspNetRoles")
                .FromTable("AspNetUserRoles").ForeignColumns("ApplicationId", "RoleId")
                .ToTable("AspNetRoles").PrimaryColumns("ApplicationId", "Id")
                .OnDelete(Rule.Cascade)
                ;
            Create.ForeignKey("FK_AspNetUserRoles_AspNetUsers")
                .FromTable("AspNetUserRoles").ForeignColumns("TenantId", "UserId")
                .ToTable("AspNetUsers").PrimaryColumns("TenantId", "Id")
                .OnDelete(Rule.Cascade)
                ;

            Create.Table("AspNetUserTokens")
                .WithColumn("TenantId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUserTokens")
                .WithColumn("UserId").AsString(450).NotNullable().PrimaryKey("PK_AspNetUserTokens")
                .WithColumn("LoginProvider").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserTokens")
                .WithColumn("Name").AsString(128).NotNullable().PrimaryKey("PK_AspNetUserTokens")
                .WithColumn("Value").AsString(int.MaxValue).Nullable()
                ;
            Create.ForeignKey("FK_AspNetUserTokens_AspNetUsers")
                .FromTable("AspNetUserTokens").ForeignColumns("TenantId", "UserId")
                .ToTable("AspNetUsers").PrimaryColumns("TenantId", "Id")
                .OnDelete(Rule.Cascade)
                ;

            Create.Index("IX_AspNetRoleClaims_RoleId").OnTable("AspNetRoleClaims").OnColumn("RoleId");
            Create.Index("IX_AspNetUserClaims_UserId").OnTable("AspNetUserClaims").OnColumn("UserId");
            Create.Index("IX_AspNetUserLogins_UserId").OnTable("AspNetUserLogins").OnColumn("UserId");
            Create.Index("IX_AspNetUserRoles_RoleId").OnTable("AspNetUserRoles").OnColumn("RoleId");

            Create.Index("EmailIndex").OnTable("AspNetUsers").OnColumn("NormalizedEmail");

            Create.Index("RoleNameIndex").OnTable("AspNetRoles")
                .OnColumn("NormalizedName").Unique()
                .OnColumn("ApplicationId").Unique();

            Create.Index("UserNameIndex").OnTable("AspNetUsers")
                .OnColumn("NormalizedUserName").Unique()
                .OnColumn("TenantId").Unique();

            Create.Index("ApplicationNameIndex").OnTable("AspNetApplications")
                .OnColumn("NormalizedName").Unique();
        }

        public override void Down()
        {
            Delete.Table("AspNetRoleClaims");
            Delete.Table("AspNetUserClaims");
            Delete.Table("AspNetUserLogins");
            Delete.Table("AspNetUserRoles");
            Delete.Table("AspNetUserTokens");
            Delete.Table("AspNetApplicationRoles");
            Delete.Table("AspNetApplications");
            Delete.Table("AspNetRoles");
            Delete.Table("AspNetUsers");
            Delete.Table("AspNetTenants");
        }
    }
}
