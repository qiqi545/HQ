// Copyright (c) Daniel Crenna & Contributor. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Lime.Web
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class DeployOnAttribute : Attribute
	{
		public DeployTarget Target { get; }

		public DeployOnAttribute(DeployTarget target)
		{
			Target = target;
		}
	}
}