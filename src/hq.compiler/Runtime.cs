using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace hq.compiler
{
    public static class Runtime
    {
        public static IEnumerable<Assembly> GetRuntimeAssemblies()
        {
            var dependencies = DependencyContext.Default.RuntimeLibraries
                .SelectMany(info => info.Dependencies);

            var assemblies = dependencies
                .Select(info => Assembly.Load(info.Name));

            return assemblies;
        }
    }
}