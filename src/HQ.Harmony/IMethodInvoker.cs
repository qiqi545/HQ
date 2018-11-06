// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Reflection;
using HQ.Remix;

namespace HQ.Harmony
{
	public interface IMethodResolver : IDisposable
	{
		MethodInfo ResolveMethod(Type serviceType, string name);
		MethodInfo ResolveMethod<T>(string name) where T : class;

		object InvokeMethod(Type serviceType, string name);
		object InvokeMethod<T>(string name) where T : class;

		Handler ResolveHandler(Type serviceType, string name);
		Handler ResolveHandler<T>(string name) where T : class;
	}
}