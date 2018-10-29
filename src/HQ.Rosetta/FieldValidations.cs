// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;
using System.Net;
using FastMember;

namespace HQ.Rosetta
{
	public static class FieldValidations
	{
		public static List<Error> MustExistOnType(Type type, IEnumerable<string> fields)
		{
			var accessor = TypeAccessor.Create(type);
			var members = accessor.GetMembers();

			var list = new List<Error>();
			foreach (var field in fields)
			{
				var valid = false;
				foreach (var member in members)
					if (field.Equals(member.Name, StringComparison.OrdinalIgnoreCase))
						valid = true;
				if (!valid)
					list.Add(new Error(string.Format(ErrorStrings.FieldToTypeFieldNameMismatch, field, type.Name),
						HttpStatusCode.BadRequest));
			}

			return list;
		}
	}
}