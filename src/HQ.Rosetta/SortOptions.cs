// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HQ.Rosetta
{
	public class SortOptions : IQueryValidator, IEnumerable<KeyValuePair<string, bool>>
	{
		public static readonly SortOptions Empty = new SortOptions();

		public List<Sort> Fields { get; } = new List<Sort>();

		public IEnumerator<KeyValuePair<string, bool>> GetEnumerator()
		{
			foreach (var field in Fields)
				yield return new KeyValuePair<string, bool>(field.Field, field.Descending);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool Validate(Type type, QueryOptions options, out IEnumerable<Error> errors)
		{
			var list = FieldValidations.MustExistOnType(type, Fields.Select(x => x.Field));
			errors = list;
			return list.Count == 0;
		}
	}
}