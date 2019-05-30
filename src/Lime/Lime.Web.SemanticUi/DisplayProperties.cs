// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Lime.Web.SemanticUi
{
	public struct DisplayProperties
	{
		public bool IsDisabled { get; set; }
		public bool IsReadOnly { get; set; }
		public bool IsVisible { get; set; }
		public bool IsHidden { get; set; }
		public bool IsRequired { get; set; }

		public string Label { get; set; }
		public string Placeholder { get; set; }
		public string Description { get; set; }

		public string Type { get; set; }
		public string Format { get; set; }
		public string Annotation { get; set; }
	}
}