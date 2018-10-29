// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace HQ.Rosetta
{
	public class MutationContext
	{
		public Type Type { get; set; }
		public MethodInfo Handle { get; set; }
		public ICollection<Error> Errors { get; } = new List<Error>();
		public dynamic Body { get; set; }

		public object Execute(IObjectRepository repository)
		{
			var parameters = new object[] {Body};

			return Handle?.Invoke(repository, parameters);
		}
	}
}