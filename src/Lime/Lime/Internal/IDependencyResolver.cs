// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Lime.Internal
{
	public interface IDependencyResolver : IDisposable
	{
		T Resolve<T>(IServiceProvider fallbackProvider) where T : class;
		T Resolve<T>(string name) where T : class;
		object Resolve(Type serviceType, IServiceProvider fallbackProvider);
		object Resolve(Type serviceType, string name);
	}
}