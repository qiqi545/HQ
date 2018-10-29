// Copyright (c) HQ.IO Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;

namespace HQ.Remix.Tests
{
	public class HandlerFactoryFixture : IDisposable
	{
		public HandlerFactoryFixture()
		{
			var resolvers = new List<IMetadataReferenceResolver> {new DefaultMetadataReferenceResolver()};
			var builder = new DefaultAssemblyBuilder(new DefaultAssemblyLoadContextProvider(), resolvers);

			var options = new NodeServicesOptions(new ServiceCollection().BuildServiceProvider())
				{ProjectPath = Directory.GetCurrentDirectory()};
			var nodeServices = NodeServicesFactory.CreateNodeServices(options);
			Factory = new HandlerFactory(builder, nodeServices, typeof(string).GetTypeInfo().Assembly); // mscorlib
		}

		public HandlerFactory Factory { get; }

		public void Dispose() { }
	}
}