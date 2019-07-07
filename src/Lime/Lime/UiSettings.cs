// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Lime
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