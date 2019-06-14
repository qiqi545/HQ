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
using HQ.Extensions.Logging;
using HQ.Extensions.Scheduling.Hooks;
using HQ.Extensions.Scheduling.Models;

namespace HQ.Platform.Functions
{
    [Description("A test function for diagnostic purposes.")]
    public class TestFunction : Before, Handler, After, Success, Failure, Halt
    {
        private readonly ISafeLogger<TestFunction> _logger;

        public TestFunction(ISafeLogger<TestFunction> logger)
        {
            _logger = logger;
        }

        public void After(ExecutionContext context)
        {
            _logger.Debug(() => $"{nameof(After)} executed.");
        }

        public void Before(ExecutionContext context)
        {
            _logger.Debug(() => $"{nameof(Before)} executed.");
        }

        public void Failure(ExecutionContext context)
        {
            _logger.Debug(() => $"{nameof(Failure)} executed.");
        }

        public void Halt(ExecutionContext context, bool immediate)
        {
            _logger.Debug(() => $"{nameof(Halt)} executed{(immediate ? " immediately" : "")}.");
        }

        public void Perform(ExecutionContext context)
        {
            _logger.Debug(() => $"{nameof(Perform)} executed.");
        }

        public void Success(ExecutionContext context)
        {
            _logger.Debug(() => $"{nameof(Success)} executed.");
        }

        public void Error(ExecutionContext context, Exception error)
        {
            _logger.Debug(() => $"{nameof(Error)} executed with error {error.Message}");
        }
    }
}
