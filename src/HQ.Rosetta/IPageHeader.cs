// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

namespace HQ.Rosetta
{
	public interface IPageHeader
	{
		long Index { get; }
		long Size { get; }
		long Count { get; }
		long TotalCount { get; }
		long TotalPages { get; }
		bool HasPreviousPage { get; }
		bool HasNextPage { get; }
	}
}