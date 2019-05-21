// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Blowdart.UI
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class ViewComponentAttribute : Attribute
	{
		public Type Type { get; }

		public ViewComponentAttribute(Type type)
		{
			Type = type;
		}
	}
}