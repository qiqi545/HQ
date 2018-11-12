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
using HQ.Common.FastMember;

namespace HQ.Lingo.Descriptor.TableDescriptor
{
    public class PropertyAccessor
    {
        private readonly TypeAccessor _accessor;

        public PropertyAccessor(TypeAccessor accessor, Type type, string name)
        {
            Type = type;
            _accessor = accessor;
            Name = name;
            ScanAttributes(accessor, name);
        }

        public string Name { get; set; }
        public Type Type { get; set; }
        public IEnumerable<Attribute> Attributes { get; set; }

        private void ScanAttributes(TypeAccessor accessor, string name)
        {
            var pi = accessor.CachedProperties.SingleOrDefault(p => p.Name == name);
            if (pi == null) return;
            Attributes = pi.GetCustomAttributes(true).Cast<Attribute>();
        }

        public object Get(dynamic example)
        {
            var result = _accessor[example, Name];
            return result;
        }

        public void Set(object instance, object value)
        {
            _accessor[instance, Name] = value;
        }
    }
}
