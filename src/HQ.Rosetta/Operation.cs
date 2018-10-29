// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

namespace HQ.Rosetta
{
	public class Operation<T>
	{
		public Operation(T data)
		{
			Data = data;
			Result = OperationResult.Executed;
		}

		public OperationResult Result { get; set; }
		public T Data { get; set; }
	}
}