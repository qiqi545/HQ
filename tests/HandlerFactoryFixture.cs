using System;
using System.Collections.Generic;
using System.Reflection;

namespace hq.compiler.tests
{
    /// <summary>
    /// We need to explicitly provide the mscorlib assembly here, whereas doing so in a platform scenario
    /// like ASP.NET would cause compilation failure. 
    /// </summary>
    public class HandlerFactoryFixture : IDisposable
    {
        public HandlerFactoryFixture()
        {
            var resolvers = new List<IMetadataReferenceResolver> { new DefaultMetadataReferenceResolver() };
            var builder = new DefaultAssemblyBuilder(new AssemblyLoadContextProvider(), resolvers);
            Factory = new HandlerFactory(builder, typeof(string).GetTypeInfo().Assembly); // mscorlib
        }

        public HandlerFactory Factory { get; }

        public void Dispose() { }
    }
}