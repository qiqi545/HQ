// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Harmony
{
	public interface IDependencyRegistrar : IDisposable
	{
		IDependencyRegistrar Register(Type type, Func<object> builder, Lifetime lifetime = Lifetime.AlwaysNew);
		IDependencyRegistrar Register<T>(Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew) where T : class;

		IDependencyRegistrar Register<T>(string name, Func<T> builder, Lifetime lifetime = Lifetime.AlwaysNew)
			where T : class;

		IDependencyRegistrar Register<T>(Func<IDependencyResolver, T> builder, Lifetime lifetime = Lifetime.AlwaysNew)
			where T : class;

		IDependencyRegistrar Register<T>(string name, Func<IDependencyResolver, T> builder,
			Lifetime lifetime = Lifetime.AlwaysNew)
			where T : class;

		IDependencyRegistrar Register<T>(T instance);
	}
}