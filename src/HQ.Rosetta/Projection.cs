// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Diagnostics;

namespace HQ.Rosetta
{
	[DebuggerDisplay("{Field} ({Type})")]
	public class Projection
	{
		public string Field { get; set; }
		public ProjectionType Type { get; set; }
	}
}