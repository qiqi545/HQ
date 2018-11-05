// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Common.Models
{
	public interface ITypeRegistry
	{
		bool Register(Type type);
		bool RegisterIfNotRegistered(Type type);
		bool TryGetType(string name, out Type type);
	}
}