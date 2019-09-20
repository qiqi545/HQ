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
using System.Diagnostics;

namespace HQ.Common.AspNetCore.Models
{
	[DebuggerDisplay("Collection: ({item.Count} items)")]
	public class MetaCollection
	{
		public dynamic info;
		public string auth;
		public dynamic protocolProfileBehavior;
		public List<MetaItem> item = new List<MetaItem>();
		public List<dynamic> @event = new List<dynamic>();
		public List<dynamic> variable = new List<dynamic>();

		public bool TryGetFolder(string name, out MetaFolder folder)
		{
			foreach (var entry in item)
			{
				if (!(entry is MetaFolder folderItem) || entry.name != name)
					continue;
				folder = folderItem;
				return true;
			}

			folder = default;
			return false;
		}
	}
}