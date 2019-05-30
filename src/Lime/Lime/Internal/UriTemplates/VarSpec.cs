// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace Lime.Internal.UriTemplates
{
	public class VarSpec
	{
		public bool Explode = false;
		public bool First = true;
		public int PrefixLength = 0;
		public StringBuilder VarName = new StringBuilder();

		public VarSpec(OperatorInfo operatorInfo) => OperatorInfo = operatorInfo;

		public OperatorInfo OperatorInfo { get; }

		public override string ToString()
		{
			return (First ? OperatorInfo.First : "") +
			       VarName
			       + (Explode ? "*" : "")
			       + (PrefixLength > 0 ? ":" + PrefixLength : "");
		}
	}
}