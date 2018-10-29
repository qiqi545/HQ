// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Reflection;

namespace HQ.Remix
{
	public interface IAssemblyBuilder
	{
		Assembly CreateInMemory(string assemblyName, string code, params Assembly[] dependencies);
		Assembly CreateInMemory(string assemblyName, string code, params string[] dependencyLocations);

		Assembly CreateOnDisk(string assemblyName, string code, string outputPath, string pdbPath = null,
			params Assembly[] dependencies);

		Assembly CreateOnDisk(string assemblyName, string code, string outputPath, string pdbPath = null,
			params string[] dependencyLocations);
	}
}