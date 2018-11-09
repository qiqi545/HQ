using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace HQ.Remix
{
    public sealed class TypeResolver : ITypeResolver
    {
        public IEnumerable<Type> ResolveByExample<TExample>()
        {
            return ResolveByExample(typeof(TExample));
        }

        private static IEnumerable<Type> ResolveByExample(Type type)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic);

            var types = assemblies.SelectMany(TryGetExportedTypes)
                .Where(x => !x.IsInterface && !x.IsAbstract)
                .OrderByDescending(type.IsAssignableFrom);

            var methodsToMatch = type.GetMethods();

            foreach (var candidateType in types)
            {
                if (type.IsAssignableFrom(candidateType))
                    yield return candidateType;
                
                foreach (var candidateMethod in candidateType.GetMethods())
                {
                    var candidateParameters = candidateMethod.GetParameters();

                    var matches = true;

                    foreach (var method in methodsToMatch)
                    {
                        if (!candidateMethod.Name.Equals(method.Name))
                        {
                            matches = false;
                            break;
                        }

                        var parameters = method.GetParameters();

                        if (candidateParameters.Length != parameters.Length)
                            continue;

                        for (var i = 0; i < parameters.Length; i++)
                        {
                            if (candidateParameters[i].ParameterType == parameters[i].ParameterType)
                                continue;
                            matches = false;
                            break;
                        }
                    }

                    if (matches)
                        yield return candidateType;
                }
            }
        }

        private static IEnumerable<Type> TryGetExportedTypes(Assembly a)
        {
            try
            {
                return a.GetExportedTypes();
            }
            catch (Exception e)
            {
                Trace.TraceInformation("Could not load exported types: {0}", e);
                return Type.EmptyTypes;
            }
        }

        public void Dispose() { }
    }
}
