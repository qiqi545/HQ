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
using HQ.Common;
using HQ.Extensions.Scheduling.Models;

namespace HQ.Extensions.Scheduling.Configuration
{
	public class BackgroundTaskOptions : FeatureToggle, IProtectedFeature, IComponentOptions
	{
		public BackgroundTaskOptions()
		{
			// System:
			DelayTasks = true;
			SleepIntervalSeconds = 10;
			CleanupIntervalSeconds = 300;
			Concurrency = 0;
			ReadAhead = 5;
			MaximumAttempts = 25;
			MaximumRuntimeSeconds = 180;
			IntervalFunction = IntervalFunction.ExponentialBackoff;
			Store = new StoreOptions();

			// Per-Task:
			DeleteOnFailure = false;
			DeleteOnSuccess = true;
			DeleteOnError = false;
			Priority = 0;
		}

		/// <summary>
		///     The function responsible for calculating the next attempt date after a tasks fails;
		///     default is 5 seconds + N.Pow(4), where N is the number of retries (i.e. exponential back-off)
		/// </summary>
		public IntervalFunction IntervalFunction { get; set; }

		/// <summary>
		///     Whether or not tasks are delayed or executed immediately; default is true
		/// </summary>
		public bool DelayTasks { get; set; }

		/// <summary>
		///     The time to delay before checking for available tasks in the backing store. Default is 10 seconds.
		/// </summary>
		public int SleepIntervalSeconds { get; set; }

		/// <summary>
		///     The time to delay before checking for hanging tasks in the backing store. Default is 3 minutes.
		/// </summary>
		public int CleanupIntervalSeconds { get; set; }

		/// <summary>
		///     The number of threads available for performing tasks; default is 0.
		///     A value of 0 defaults to the number of logical processors.
		/// </summary>
		public int Concurrency { get; set; }

		/// <summary>
		///     The number of jobs to pull at once when searching for available tasks; default is 5.
		/// </summary>
		public int ReadAhead { get; set; }

		/// <summary>
		///     The maximum number of attempts made before failing a task permanently; default is 25.
		/// </summary>
		public int MaximumAttempts { get; set; }

		/// <summary>
		///     The maximum time each task is allowed before being cancelled; default is 3 minutes.
		/// </summary>
		public int MaximumRuntimeSeconds { get; set; }

		/// <summary>
		///     Whether or not failed tasks are deleted from the backend store; default is false.
		/// </summary>
		public bool DeleteOnFailure { get; set; }

		/// <summary>
		///     Whether or not successful tasks are deleted from the backend store; default is true.
		/// </summary>
		public bool DeleteOnSuccess { get; set; }

		/// <summary>
		///     Whether or not tasks that throw exceptions are deleted from the backend store; default is false.
		/// </summary>
		public bool DeleteOnError { get; set; }

		/// <summary>
		///     The default priority level for newly created scheduled tasks that don't specify a priority; default is 0, or
		///     highest priority
		/// </summary>
		public int Priority { get; set; }

		public StoreOptions Store { get; set; }
		public string RootPath { get; set; } = "/ops";
		public string Scheme { get; set; } = Constants.Security.Schemes.PlatformBearer;
		public string Policy { get; set; } = Constants.Security.Policies.ManageBackgroundTasks;

		/// <summary> Set task values that have defaults if not provided by the user. </summary>
		public void ProvisionTask(BackgroundTask task)
		{
			task.MaximumRuntime = task.MaximumRuntime ?? TimeSpan.FromSeconds(MaximumRuntimeSeconds);
			task.MaximumAttempts = task.MaximumAttempts ?? MaximumAttempts;
			task.DeleteOnSuccess = task.DeleteOnSuccess ?? DeleteOnSuccess;
			task.DeleteOnFailure = task.DeleteOnFailure ?? DeleteOnFailure;
			task.DeleteOnError = task.DeleteOnError ?? DeleteOnError;
		}
	}
}