// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

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

		public string Name { get; private set; }

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

			public string Message { get; private set; }

			public Exception Error { get; private set; }

			public bool IsHealthy { get; private set; }

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