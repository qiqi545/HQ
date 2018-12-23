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

namespace HQ.Cadence
{
    /// <summary>
    ///     A template class for an encapsulated service health check
    /// </summary>
    public class HealthCheck
    {
        private readonly Func<Result> _check;

        public HealthCheck(string name, Func<Result> check)
        {
            Name = name;
            _check = check;
        }

        public static Result Healthy => Result.Healthy;

        public string Name { get; }

        public static Result Unhealthy(string message)
        {
            return Result.Unhealthy(message);
        }

        public static Result Unhealthy(Exception error)
        {
            return Result.Unhealthy(error);
        }

        public Result Execute()
        {
            try
            {
                return _check();
            }
            catch (Exception e)
            {
                return Result.Unhealthy(e);
            }
        }

        public sealed class Result
        {
            private Result(bool isHealthy, string message, Exception error)
            {
                IsHealthy = isHealthy;
                Message = message;
                Error = error;
            }

            public static Result Healthy { get; } = new Result(true, null, null);

            public string Message { get; }

            public Exception Error { get; }

            public bool IsHealthy { get; }

            public static Result Unhealthy(string errorMessage)
            {
                return new Result(false, errorMessage, null);
            }

            public static Result Unhealthy(Exception error)
            {
                return new Result(false, error.Message, error);
            }
        }
    }
}
