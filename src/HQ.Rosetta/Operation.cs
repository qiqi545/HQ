// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Collections.Generic;
using System.Linq;

namespace HQ.Rosetta
{
	public class Operation
	{
		public OperationResult Result { get; set; }
		public bool Succeeded => Result == OperationResult.Succeeded || Result == OperationResult.SucceededWithErrors;
		public bool HasErrors => Errors?.Count() > 0;
		public IEnumerable<Error> Errors { get; set; }

		public Operation()
		{
			Result = OperationResult.Succeeded;
		}

		public Operation(IEnumerable<Error> errors) : this()
		{
			Errors = errors;
		}
	}
	
	
}