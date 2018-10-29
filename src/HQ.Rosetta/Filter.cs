// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Diagnostics;

namespace HQ.Rosetta
{
	[DebuggerDisplay("{Type} {Field} {Operator} {Value}")]
	public class Filter
	{
		public FilterType Type { get; set; }
		public string Field { get; set; }
		public FilterOperator Operator { get; set; }
		public object Value { get; set; }
	}
}