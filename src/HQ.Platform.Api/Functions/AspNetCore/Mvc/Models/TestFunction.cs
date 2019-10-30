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
using System.ComponentModel;
using System.Threading.Tasks;
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling.Hooks;
using HQ.Extensions.Scheduling.Models;

namespace HQ.Platform.Api.Functions.AspNetCore.Mvc.Models
{
	[Description("A test function for diagnostic purposes.")]
	public class TestFunction : Before, Handler, After, Success, Failure, Halt, Error
	{
		private readonly ISafeLogger<TestFunction> _logger;

		public TestFunction(ISafeLogger<TestFunction> logger) => _logger = logger;

		public Task AfterAsync(ExecutionContext context)
		{
			_logger.Debug(() => $"{nameof(After)} executed.");
			return Task.CompletedTask;
		}

		public Task BeforeAsync(ExecutionContext context)
		{
			_logger.Debug(() => $"{nameof(BeforeAsync)} executed.");
			return Task.CompletedTask;
		}

		public Task ErrorAsync(ExecutionContext context, Exception error)
		{
			_logger.Debug(() => $"{nameof(Error)} executed with error {error.Message}");
			return Task.CompletedTask;
		}

		public Task FailureAsync(ExecutionContext context)
		{
			_logger.Debug(() => $"{nameof(FailureAsync)} executed.");
			return Task.CompletedTask;
		}

		public Task HaltAsync(ExecutionContext context, bool immediate)
		{
			_logger.Debug(() => $"{nameof(Halt)} executed{(immediate ? " immediately" : "")}.");
			return Task.CompletedTask;
		}

		public Task PerformAsync(ExecutionContext context)
		{
			_logger.Debug(() => $"{nameof(PerformAsync)} executed.");
			if (context.TryGetData("Success", out var succeed) && succeed is bool flag && flag)
				context.Succeed();
			else
				context.Fail();
			return Task.CompletedTask;
		}

		public Task SuccessAsync(ExecutionContext context)
		{
			_logger.Debug(() => $"{nameof(SuccessAsync)} executed.");
			return Task.CompletedTask;
		}
	}
}