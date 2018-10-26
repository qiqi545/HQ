// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.IO;

namespace HQ.Flow
{
	/// <summary>
	///     A contract for protocol negotiation between produces and consumers.
	/// </summary>
	public interface ISerializer
	{
		Stream SerializeToStream<T>(T @event);
		T DeserializeFromStream<T>(Stream stream);
	}
}