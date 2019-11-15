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

namespace HQ.Integration.SqlServer.Scheduling
{
	[Migration(0)]
	public class CreateBackgroundTasksSchema : Migration
	{
		public override void Up()
		{
			const string schema = "dbo";

			Execute.Sql($"CREATE SEQUENCE [{schema}].[{nameof(BackgroundTask)}_Id] START WITH 1 INCREMENT BY 1");

			Create.Table(nameof(BackgroundTask))
				.WithColumn("Id")
				.AsCustom($"INT DEFAULT(NEXT VALUE FOR [{schema}].[{nameof(BackgroundTask)}_Id]) PRIMARY KEY CLUSTERED")
				.WithColumn("CorrelationId").AsGuid().NotNullable()
				.WithColumn("Priority").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("Attempts").AsInt32().NotNullable().WithDefaultValue(0)
				.WithColumn("Handler").AsString(int.MaxValue).NotNullable()
				.WithColumn("RunAt").AsDateTimeOffset().NotNullable()
				.WithColumn("MaximumRuntime").AsTime().NotNullable()
				.WithColumn("MaximumAttempts").AsInt32().NotNullable()
				.WithColumn("DeleteOnSuccess").AsBoolean().NotNullable()
				.WithColumn("DeleteOnFailure").AsBoolean().NotNullable()
				.WithColumn("DeleteOnError").AsBoolean().NotNullable()
				.WithColumn("CreatedAt").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
				.WithColumn("LastError").AsString().Nullable()
				.WithColumn("FailedAt").AsDateTimeOffset().Nullable()
				.WithColumn("SucceededAt").AsDateTimeOffset().Nullable()
				.WithColumn("LockedAt").AsDateTimeOffset().Nullable()
				.WithColumn("LockedBy").AsString().Nullable()
				.WithColumn("Expression").AsAnsiString().Nullable()
				.WithColumn("Start").AsDateTimeOffset().NotNullable()
				.WithColumn("ContinueOnSuccess").AsBoolean().NotNullable()
				.WithColumn("ContinueOnFailure").AsBoolean().NotNullable()
				.WithColumn("ContinueOnError").AsBoolean().NotNullable()
				.WithColumn("End").AsDateTimeOffset().Nullable()
				.WithColumn("Data").AsString(int.MaxValue).Nullable()
				;
			Create.Table($"{nameof(BackgroundTask)}_Tag")
				.WithColumn("Id").AsInt32().PrimaryKey().Identity()
				.Unique() // CLUSTERED INDEX + UNIQUE (Faster Lookups)
				.WithColumn("Name").AsString().NotNullable().Indexed() // ORDER BY ScheduledTask_Tag.Name ASC
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
			Execute.Sql($"DROP TABLE [{nameof(BackgroundTask)}_Tags]");
			Execute.Sql($"DROP TABLE [{nameof(BackgroundTask)}_Tag]");
			Execute.Sql($"DROP TABLE [{nameof(BackgroundTask)}]");
			Execute.Sql($"DROP SEQUENCE [{nameof(BackgroundTask)}_Id]");
		}
	}
}