// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Diagnostics;

namespace HQ.Rosetta
{
	[DebuggerDisplay("{Field} {ToString}")]
	public class Sort
	{
		public string Field { get; set; }
		public bool Descending { get; set; }

		public override string ToString()
		{
			return $"{(Descending ? "-" : "+")}{Field}";
		}
	}
}