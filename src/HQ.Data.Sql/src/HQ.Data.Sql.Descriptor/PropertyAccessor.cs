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
using System.Linq;
using System.Reflection;
using HQ.Common.FastMember;

namespace HQ.Data.Sql.Descriptor
{
    public class PropertyAccessor
    {
        private readonly TypeAccessor _accessor;
        private readonly Member _member;

        public PropertyAccessor(TypeAccessor accessor, Type type, string name)
        {
            Type = type;
            _accessor = accessor;

            Name = name;
            Info = accessor.CachedMembers.SingleOrDefault(m => m.Name == name) as PropertyInfo;

            var member = accessor.GetMembers().SingleOrDefault(p => p.Name == name);
            if (member == null) return;
            _member = member;
        }

        public string Name { get; set; }
        public Type Type { get; set; }
        public PropertyInfo Info { get; set; }

        public bool HasAttribute<T>() where T : Attribute
        {
            return _member.IsDefined(typeof(T));
        }

        public Attribute GetAttribute<T>() where T : Attribute
        {
            return _member.GetAttribute(typeof(T), true);
        }

        public object Get(object instance)
        {
            return _accessor[instance, Name];
        }

        public void Set(object instance, object value)
        {
            _accessor[instance, Name] = value;
        }
    }
}
