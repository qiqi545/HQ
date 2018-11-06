// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using HQ.Common.Extensions;

namespace HQ.Remix
{
	public class MethodFactory
	{
		public static MethodFactory Default = new MethodFactory();

		private readonly IDictionary<MethodInfo, ParameterInfo[]> _methodInfoParameters =
			new ConcurrentDictionary<MethodInfo, ParameterInfo[]>();

		private readonly IDictionary<NameAndType, MethodInfo> _methods =
			new ConcurrentDictionary<NameAndType, MethodInfo>();

		public MethodInfo GetOrCacheMethodForTypeAndName(Type type, string name,
			StringComparison comparison = StringComparison.OrdinalIgnoreCase)
		{
			// type(name)->method
			var key = new NameAndType(name, type);
			if (!_methods.TryGetValue(key, out var method))
				_methods[key] = method = type.GetWidestMethod(name, comparison);
			return method;
		}

		public ParameterInfo[] GetOrCacheParametersForMethod(MethodInfo method)
		{
			// method->parameters
			if (!_methodInfoParameters.TryGetValue(method, out var parameters))
				_methodInfoParameters[method] = parameters = method.GetParameters();
			return parameters;
		}
	}
}