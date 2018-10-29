// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Reflection;
using Microsoft.CodeAnalysis;

namespace HQ.Remix
{
	public class DefaultMetadataReferenceResolver : IMetadataReferenceResolver
	{
		public MetadataReference Resolve(Assembly assembly)
		{
			return MetadataReference.CreateFromFile(assembly.Location);
		}

		public MetadataReference Resolve(string location)
		{
			return MetadataReference.CreateFromFile(location);
		}
	}
}