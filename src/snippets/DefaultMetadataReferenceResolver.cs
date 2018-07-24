using System.Reflection;
using Microsoft.CodeAnalysis;
using snippets;

namespace hq.compiler
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