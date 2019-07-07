// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Lime
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
	public class MetaAttribute : Attribute
	{
		public MetaAttribute(string name, string content)
		{
			Name = name;
			Content = content;
		}

		public string Name { get; }
		public string Content { get; }
	}
}