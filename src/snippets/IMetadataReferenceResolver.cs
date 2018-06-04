using System.Reflection;
using Microsoft.CodeAnalysis;

namespace snippets
{
    public interface IMetadataReferenceResolver
    {
        MetadataReference Resolve(Assembly assembly);
    }
}