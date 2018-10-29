// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;

namespace HQ.Rosetta
{
	public class FieldOptions : IQueryValidator
	{
		public List<string> Fields { get; } = new List<string>();

		public bool Validate(Type type, QueryOptions options, out IEnumerable<Error> errors)
		{
			var list = FieldValidations.MustExistOnType(type, Fields);
			errors = list;
			return list.Count == 0;
		}
	}
}