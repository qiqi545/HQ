// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Lime.Web.SemanticUi
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class SemanticUiAttribute : UiSystemAttribute
	{
		public SemanticUiAttribute() : base(typeof(SemanticUi)) { }
	}
}