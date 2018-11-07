// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;

namespace HQ.Harmony.Tests
{
	public class HarmonyContainerFixture : IDisposable
	{
		public HarmonyContainerFixture()
		{
			C = new HarmonyContainer();
		}

		public IContainer C { get; }

		public void Dispose()
		{
			C.Dispose();
		}
	}
}