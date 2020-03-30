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

using System.Diagnostics;
using System.Threading.Tasks;
using ActiveScheduler.Hooks;
using ActiveScheduler.Models;
using Microsoft.Extensions.Logging;

namespace HQ.Platform.Tests.Scheduling.Handlers
{
	public class StaticCountingHandler : Handler
	{
		private readonly ILogger<StaticCountingHandler> _logger;
		private readonly ILoggerFactory _factory;

		public static int Count { get; set; }
        
		public static object Data { get; set; }

		public StaticCountingHandler(ILogger<StaticCountingHandler> logger, ILoggerFactory factory)
		{
			_logger = logger;
			_factory = factory;
		}

        public Task PerformAsync(ExecutionContext context)
        {
	        if (!context.TryGetData("Foo", out var data))
		        Trace.WriteLine("missing data");
	        Data = data;
			Count++;
            return Task.CompletedTask;
        }
    }
}


