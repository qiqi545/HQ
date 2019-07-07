// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Lime
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class ViewComponentAttribute : Attribute
	{
		public ViewComponentAttribute(Type type) => Type = type;

		public Type Type { get; }
	}
}