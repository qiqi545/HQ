using System.Reflection;
using Microsoft.CodeAnalysis;

namespace hq.compiler
{
    public class DefaultMetadataReferenceResolver : IMetadataReferenceResolver
    {
        public MetadataReference Resolve(Assembly assembly)
        {
            return MetadataReference.CreateFromFile(assembly.Location);
        }
    }
}