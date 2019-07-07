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
using System.Reflection;

namespace HQ.UI
{
	public class UiSettings
	{
		public UiSettings()
		{
			DefaultPageMethodName = "Index";
			DefaultPageTitle = "My Application";
		}

		public UiData Data { get; set; }
		public Assembly[] ComponentAssemblies { get; set; }

		public UiSystem DefaultSystem { get; set; }
		public string DefaultPageMethodName { get; set; }
		public string DefaultPageTitle { get; set; }

		public void AutoRegisterComponentAssemblies()
		{
			var list = new List<Assembly>();
			if (ComponentAssemblies != null)
				list.AddRange(ComponentAssemblies);
			list.Add(Assembly.GetCallingAssembly());
			list.Add(Assembly.GetEntryAssembly());
			ComponentAssemblies = list.ToArray();
		}
	}
}