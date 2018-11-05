// Copyright (c) HQ.IO Corporation. All rights reserved.
// Usage is strictly forbidden if not under license.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace HQ.Common.AspNetCore
{
	public class DynamicControllerApplicationPart : ApplicationPart, IApplicationPartTypeProvider
	{
		public override string Name => nameof(DynamicControllerApplicationPart);
		public IEnumerable<TypeInfo> Types { get; }

		public DynamicControllerApplicationPart(IEnumerable<TypeInfo> types)
		{
			Types = types;
		}
	}
}