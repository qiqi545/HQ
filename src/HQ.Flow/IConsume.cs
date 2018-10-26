// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Threading.Tasks;

namespace HQ.Flow
{
	/// <summary>
	///     An event handler; contains the processing or storage logic for when an event is received
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IConsume<in T>
	{
		Task<bool> HandleAsync(T message);
	}

	/// <summary>
	///     Allows scoped programmability for a given consumer.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IConsumeScoped<in T> : IConsume<T>
	{
		/// <summary>
		///     Runs before handling a message.
		///     If this method returns <code>false</code>, the handler is not invoked.
		/// </summary>
		/// <returns></returns>
		bool Before();

		/// <summary>
		///     Runs after handling a message.
		///     The result returned fromm HandleAsync is replaced with the return of this method.
		/// </summary>
		/// <param name="result">The value returned from the handler after invoking.</param>
		/// <returns></returns>
		bool After(bool result);
	}
}