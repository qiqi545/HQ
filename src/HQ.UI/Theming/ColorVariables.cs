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

using System.Collections.Generic;
using System.Drawing;

namespace HQ.UI.Theming
{
	public class ColorVariables : Dictionary<NamedColors, Color>
	{
		public ColorVariables()
		{
			Add(NamedColors.Red, "#B03060".ToColor());
			Add(NamedColors.Orange, "#FE9A76".ToColor());
			Add(NamedColors.Yellow, "#FFD700".ToColor());
			Add(NamedColors.Olive, "#32CD32".ToColor());
			Add(NamedColors.Green, "#016936".ToColor());
			Add(NamedColors.Teal, "#008080".ToColor());
			Add(NamedColors.Blue, "#0E6EB8".ToColor());
			Add(NamedColors.Violet, "#EE82EE".ToColor());
			Add(NamedColors.Purple, "#B413EC".ToColor());
			Add(NamedColors.Pink, "#FF1493".ToColor());
			Add(NamedColors.Brown, "#A52A2A".ToColor());
			Add(NamedColors.Grey, "#A0A0A0".ToColor());
			Add(NamedColors.Black, "#000000".ToColor());

			Add(NamedColors.Primary, "#B03060".ToColor());
			Add(NamedColors.Secondary, "#B03060".ToColor());
		}
	}
}