// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Flow
{
	[Serializable]
	public class AlreadyInitializedException : Exception
	{
		public AlreadyInitializedException() : base(
			"StateProvider was already setup, and clear was not called before calling setup again")
		{
		}
	}
}