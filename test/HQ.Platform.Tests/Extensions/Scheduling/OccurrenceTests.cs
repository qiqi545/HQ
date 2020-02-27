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
using ActiveScheduler;
using ActiveScheduler.Models;
using HQ.Common;
using HQ.Platform.Tests.Fixtures;
using HQ.Test.Sdk;

namespace HQ.Platform.Tests.Extensions.Scheduling
{
	public class OccurrenceTests : UnitUnderTest
	{
		private readonly IBackgroundTaskStore _store;

		public OccurrenceTests()
		{
			_store = new InMemoryBackgroundTaskStore(()=> DateTimeOffset.Now);
		}

		[Test]
		public void Occurrence_is_in_UTC()
		{
            var task = new BackgroundTask {
	            RunAt = _store.GetTaskTimestamp(), 
	            Expression = CronTemplates.Daily(1, 3, 30)};

            var next = task.NextOccurrence;
			Assert.NotNull(next);
			Assert.True(next.Value.Hour == 3);
			Assert.Equal(next.Value.Hour, next.Value.UtcDateTime.Hour);
		}
	}
}
