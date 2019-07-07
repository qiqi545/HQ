// Copyright (c) Daniel Crenna & Contributors. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Buffers;
using System.Reflection;
using Microsoft.Extensions.ObjectPool;

namespace Lime.Internal
{
	internal static class Pools
	{
		public static readonly ArrayPool<Assembly> AssemblyPool = ArrayPool<Assembly>.Create();

		public static readonly ObjectPool<UiAction>
			ActionPool = new DefaultObjectPool<UiAction>(new ActionPoolPolicy());

		public static NoContainer AutoResolver { get; set; }

		internal class ActionPoolPolicy : IPooledObjectPolicy<UiAction>
		{
			public UiAction Create()
			{
				return new UiAction();
			}

			public bool Return(UiAction action)
			{
				action.Arguments = null;
				action.MethodName = null;
				return true;
			}
		}
	}
}