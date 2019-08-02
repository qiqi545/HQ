#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

namespace HQ.UI.Web.SemanticUi
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