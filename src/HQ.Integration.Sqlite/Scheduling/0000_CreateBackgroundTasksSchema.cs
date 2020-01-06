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

using FluentMigrator;
using HQ.Extensions.Scheduling.Models;

namespace HQ.Integration.Sqlite.Scheduling
{
	[Migration(0)]
	public class CreateBackgroundTasksSchema : Migration
	{
		public override void Up()
		{
			Create.Table(nameof(BackgroundTask))
				.WithColumn("Id").AsInt32().PrimaryKey().Identity()
				.WithColumn("Priority").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("Attempts").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("Handler").AsString(int.MaxValue).NotNullable()
				.WithColumn("RunAt").AsDateTime().NotNullable()
				.WithColumn("MaximumRuntime").AsTime().NotNullable()
				.WithColumn("MaximumAttempts").AsInt32().NotNullable()
				.WithColumn("DeleteOnSuccess").AsBoolean().NotNullable()
				.WithColumn("DeleteOnFailure").AsBoolean().NotNullable()
				.WithColumn("DeleteOnError").AsBoolean().NotNullable()
				.WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
				.WithColumn("LastError").AsString().Nullable()
				.WithColumn("FailedAt").AsDateTime().Nullable()
				.WithColumn("SucceededAt").AsDateTime().Nullable()
				.WithColumn("LockedAt").AsDateTime().Nullable()
				.WithColumn("LockedBy").AsString().Nullable()
				.WithColumn("Expression").AsAnsiString().Nullable()
				.WithColumn("Start").AsDateTime().NotNullable()
				.WithColumn("ContinueOnSuccess").AsBoolean().NotNullable()
				.WithColumn("ContinueOnFailure").AsBoolean().NotNullable()
				.WithColumn("ContinueOnError").AsBoolean().NotNullable()
				.WithColumn("End").AsDateTime().Nullable()
				;
			Create.Table($"{nameof(BackgroundTask)}_Tag")
				.WithColumn("Id").AsInt32().PrimaryKey().Identity()
				.Unique() // CLUSTERED INDEX + UNIQUE (Faster Lookups)
				.WithColumn("Name").AsString().NotNullable().Unique()
				;
			Create.Table($"{nameof(BackgroundTask)}_Tags")
				.WithColumn("Id").AsInt32().PrimaryKey().Identity()
				.Unique() // CLUSTERED INDEX + UNIQUE (Faster Lookups)
				.WithColumn($"{nameof(BackgroundTask)}Id").AsInt32().ForeignKey($"{nameof(BackgroundTask)}", "Id")
				.NotNullable().Indexed()
				.WithColumn("TagId").AsInt32().ForeignKey($"{nameof(BackgroundTask)}_Tag", "Id").NotNullable().Indexed()
				;
		}

		public override void Down()
		{
			Delete.Table($"{nameof(BackgroundTask)}_Tags");
			Delete.Table($"{nameof(BackgroundTask)}_Tag");
			Delete.Table($"{nameof(BackgroundTask)}");
		}
	}
}