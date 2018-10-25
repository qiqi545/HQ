// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections;
using System.Collections.Generic;

namespace HQ.Harmony
{
	public interface IDependencyResolver : IDisposable
	{
		T Resolve<T>() where T : class;
		object Resolve(Type serviceType);
		IEnumerable<T> ResolveAll<T>() where T : class;
		IEnumerable ResolveAll(Type serviceType);
		T Resolve<T>(string name) where T : class;
		object Resolve(string name, Type serviceType);
	}
}