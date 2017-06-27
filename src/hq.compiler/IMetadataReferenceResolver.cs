using System.Reflection;
using Microsoft.CodeAnalysis;

namespace hq.compiler
{
    public interface IMetadataReferenceResolver
    {
        MetadataReference Resolve(Assembly assembly);
    }
}