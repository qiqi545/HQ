// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Threading.Tasks;

namespace HQ.Flow
{
	/// <summary>
	///     An message publisher allows eventing to one or more consumers.
	/// </summary>
	public interface IMessagePublisher : IDisposable
	{
		Task<bool> PublishAsync(object message);
		bool Publish(object message);
	}
}