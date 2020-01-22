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
	[Migration(1)]
	public class AddBackgroundTaskCorrelationId : Migration
	{
		public override void Up()
		{
			Alter.Table(nameof(BackgroundTask))
				.AddColumn(nameof(BackgroundTask.CorrelationId)).AsGuid().NotNullable()
				;
		}

		public override void Down()
		{
			Delete.Column(nameof(BackgroundTask.CorrelationId)).FromTable(nameof(BackgroundTask));
		}
	}
}