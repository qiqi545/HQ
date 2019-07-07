// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Lime.Internal.UriTemplates
{
	public class OperatorInfo
	{
		public bool Default { get; set; }
		public string First { get; set; }
		public char Separator { get; set; }
		public bool Named { get; set; }
		public string IfEmpty { get; set; }
		public bool AllowReserved { get; set; }
	}
}