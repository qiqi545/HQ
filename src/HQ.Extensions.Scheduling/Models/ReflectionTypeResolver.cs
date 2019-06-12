#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HQ.Extensions.Scheduling.Models
{
    public class ReflectionTypeResolver : ITypeResolver
    {
        private readonly IEnumerable<Assembly> _assemblies;

        public ReflectionTypeResolver(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
        }

        public ReflectionTypeResolver() : this(AppDomain.CurrentDomain.GetAssemblies())
        {
        }

        public Type FindByName(string typeName)
        {
            var mscorlib = typeof(object).GetTypeInfo().Assembly;
            var loadedTypes = _assemblies.Where(a => !a.IsDynamic && a != mscorlib).SelectMany(a => a.GetTypes());
            return loadedTypes.FirstOrDefault(t =>
                t.FullName != null && t.FullName.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<Type> GetAncestors(Type type)
        {
            foreach (var i in type?.GetInterfaces() ?? Enumerable.Empty<Type>())
            {
                yield return i;
            }

            if (type?.BaseType == null || type.BaseType == typeof(object))
            {
                yield break;
            }

            var baseType = type.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }
    }
}
