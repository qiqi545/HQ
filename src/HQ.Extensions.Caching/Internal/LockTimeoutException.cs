using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;

namespace HQ.Extensions.Caching.Internal
{
	/// <summary>
	///     Thrown when a lock times out.
	/// </summary>
	[Serializable]
	public class LockTimeoutException : Exception
	{
		/// <summary>
		///     Creates a new <see cref="LockTimeoutException" /> instance.
		/// </summary>
		public LockTimeoutException() : base("Timeout waiting for lock")
		{
		}

		/// <summary>
		///     Constructor.
		/// </summary>
		/// <param name="message"></param>
		public LockTimeoutException(string message) : base(message)
		{
		}

		/// <summary>
		///     Constructor.
		/// </summary>
		/// <param name="message"></param>
		/// <param name="innerException"></param>
		public LockTimeoutException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		///     Constructor.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected LockTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		/// <summary>
		///     Returns a string representation of the exception.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			var toString = base.ToString();
#if DEBUG
			if (blockingStackTrace != null)
			{
				toString += "\n-------Blocking Stack Trace--------\n" + blockingStackTrace;
			}
#endif
			return toString;
		}
#if DEBUG
		private readonly object lockTarget;
		private StackTrace blockingStackTrace;
		private static readonly Hashtable failedLockTargets = new Hashtable();

		/// <summary>
		///     Sets the stack trace for the given lock target
		///     if an error occurred.
		/// </summary>
		/// <param name="lockTarget">Lock target.</param>
		public static void ReportStackTraceIfError(object lockTarget)
		{
			lock (failedLockTargets)
			{
				if (failedLockTargets.ContainsKey(lockTarget))
				{
					var waitHandle = failedLockTargets[lockTarget] as ManualResetEvent;
					if (waitHandle != null)
					{
						waitHandle.Set();
					}

					failedLockTargets[lockTarget] = new StackTrace();
					//Also. if you don't call GetBlockingStackTrace()
					//the lockTarget doesn't get removed from the hash 
					//table and so we'll always think there's an error
					//here (though no locktimeout exception is thrown).
				}
			}
		}

		/// <summary>
		///     Creates a new <see cref="LockTimeoutException" /> instance.
		/// </summary>
		/// <remarks>Use this exception.</remarks>
		/// <param name="lockTarget">Object we tried to lock.</param>
		public LockTimeoutException(object lockTarget)
			: base("Timeout waiting for lock")
		{
			lock (failedLockTargets)
			{
				// This is safer in case somebody forgot to remove 
				// the lock target.
				var waitHandle = new ManualResetEvent(false);
				failedLockTargets[lockTarget] = waitHandle;
			}

			this.lockTarget = lockTarget;
		}

		/// <summary>
		///     Stack trace of the thread that holds a lock on the object
		///     this lock is attempting to acquire when it fails.
		/// </summary>
		/// <param name="timeout">Number of milliseconds to wait for the blocking stack trace.</param>
		public StackTrace GetBlockingStackTrace(int timeout)
		{
			if (timeout < 0)
				throw new InvalidOperationException(
					"We'd all like to be able to go back in time, but this is not allowed. Please choose a positive wait time.");

			ManualResetEvent waitHandle;
			lock (failedLockTargets)
			{
				waitHandle = failedLockTargets[lockTarget] as ManualResetEvent;
			}

			if (timeout > 0 && waitHandle != null)
			{
				waitHandle.WaitOne(timeout, false);
			}

			lock (failedLockTargets)
			{
				//Hopefully by now we have a stack trace.
				blockingStackTrace = failedLockTargets[lockTarget] as StackTrace;
			}

			return blockingStackTrace;
		}
#endif
	}
}