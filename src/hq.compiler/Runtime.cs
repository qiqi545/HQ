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
            var assemblies = DependencyContext.Default.RuntimeLibraries
                .SelectMany(info => info.Assemblies)
                .Select(info => Assembly.Load(info.Name));

            return assemblies;
        }
    }
}