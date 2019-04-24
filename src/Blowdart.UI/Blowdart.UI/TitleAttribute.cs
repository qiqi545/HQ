// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Blowdart.UI
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public class TitleAttribute : MetaAttribute
	{
		public TitleAttribute(string title) : base("title", title) { }
	}
}