// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Lime
{
	[AttributeUsage(AttributeTargets.Method)]
	public class HandlerAttribute : Attribute
	{
		public HandlerAttribute(string template) => Template = template;

		public string Template { get; }
	}
}