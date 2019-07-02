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

using System;
using HQ.Extensions.Scheduling.Models;
using HQ.Test.Sdk;
using HQ.Test.Sdk.Xunit;

namespace HQ.Extensions.Scheduling.Tests
{
	public class OccurrenceTests : UnitUnderTest
	{
		[Test]
		public void Occurrence_is_in_UTC()
		{
			var task = new BackgroundTask();
			task.RunAt = DateTimeOffset.UtcNow;

			task.Expression = CronTemplates.Daily(1, 3, 30);
			var next = task.NextOccurrence;
			Assert.NotNull(next);
			Assert.True(next.Value.Hour == 3);
			Assert.Equal(next.Value.Hour, next.Value.UtcDateTime.Hour);
		}
	}
}
