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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using HQ.Common;

namespace HQ.Platform.Identity.Stores.Sql
{
    public class TypeRegistry : ITypeRegistry
    {
        private readonly ConcurrentDictionary<string, Type> _typesByName = new ConcurrentDictionary<string, Type>();

        public bool Register(Type type)
        {
            Debug.Assert(type.AssemblyQualifiedName != null, "type.AssemblyQualifiedName != null");
            if (_typesByName.TryGetValue(type.AssemblyQualifiedName, out _))
            {
                throw new ArgumentException("type is already registered");
            }

            _typesByName.AddOrUpdate(type.AssemblyQualifiedName, s => type, (s, t) => t);
            return true;
        }

        public bool TryRegister(Type type)
        {
            Debug.Assert(type.AssemblyQualifiedName != null, "type.AssemblyQualifiedName != null");
            if (_typesByName.TryGetValue(type.AssemblyQualifiedName, out _))
            {
                return false;
            }

            _typesByName.AddOrUpdate(type.AssemblyQualifiedName, s => type, (s, t) => t);
            return true;
        }

        public bool TryGetType(string name, out Type type)
        {
            // exact match
            if (_typesByName.TryGetValue(name, out type))
            {
                return true;
            }

            // context-free name match
            type = _typesByName.FirstOrDefault(x => x.Value.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Value;
            return type != null;
        }
    }
}
