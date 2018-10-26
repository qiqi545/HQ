// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Threading;

namespace HQ.Flow.Examples
{
	public interface IExample
	{
		void Execute(AutoResetEvent block);
	}
}