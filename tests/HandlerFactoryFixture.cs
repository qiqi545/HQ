using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using hq.compiler;
using Microsoft.AspNetCore.NodeServices;
using Microsoft.Extensions.DependencyInjection;

namespace snippets.tests
{
    /// <inheritdoc />
    /// <summary>
    /// We need to explicitly provide the mscorlib assembly here, whereas doing so in a platform scenario
    /// like ASP.NET would cause compilation failure. 
    /// </summary>
    public class HandlerFactoryFixture : IDisposable
    {
        public HandlerFactoryFixture()
        {
            var resolvers = new List<IMetadataReferenceResolver> { new DefaultMetadataReferenceResolver() };
            var builder = new DefaultAssemblyBuilder(new DefaultAssemblyLoadContextProvider(), resolvers);

	        var options = new NodeServicesOptions(new ServiceCollection().BuildServiceProvider()) { ProjectPath = Directory.GetCurrentDirectory() };
	        var nodeServices = NodeServicesFactory.CreateNodeServices(options);
			Factory = new HandlerFactory(builder, nodeServices, typeof(string).GetTypeInfo().Assembly); // mscorlib
        }

        public HandlerFactory Factory { get; }

        public void Dispose() { }
    }
}