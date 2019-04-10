// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using TypeKitchen;

namespace Blowdart.UI.Web
{
    public class Attributes : DynamicObject
    {
        internal IReadOnlyDictionary<string, object> Inner { get; }

        private Attributes(IReadOnlyDictionary<string, object> inner)
        {
            Inner = inner;
        }

        private Attributes() { }

        public static Attributes Attr(object attr)
        {
            if(attr is string)
                throw new HtmlException($"You provided a string literal for an attribute object. Did you mean new {{ @class = \"{attr}\" }} ?");

            var hash = CreateHash(attr);

            return new Attributes(hash);
        }

        public static Attributes Attr(params object[] attr)
        {
            if (attr.Length == 0)
                return new Attributes();

            var instance = attr[0];

            var hash = instance is Attributes full ? full.Inner :
                instance is null ? new Dictionary<string, object>() : ReadAccessor.Create(instance.GetType()).AsReadOnlyDictionary(instance);

            for (var i = 1; i < attr.Length; i++)
            {
                switch (attr[i])
                {
                    case string _:
                        throw new HtmlException($"You provided a string literal for an attribute object. Did you mean new {{ @class = \"{attr[i]}\" }} ?");

                    case Attributes other:
                        hash = hash.Concat(other.Inner).ToDictionary(k => k.Key, v => v.Value);
                        break;
                    default:
                        hash = hash.Concat(CreateHash(attr[i])).ToDictionary(k => k.Key, v => v.Value);
                        break;
                }
            }
            return new Attributes(hash);
        }

        private static IReadOnlyDictionary<string, object> CreateHash(object attr)
        {
            if (attr == null)
                return new Dictionary<string, object>();

            var type = attr.GetType();
            var accessor = ReadAccessor.Create(type);

            var hash = accessor.AsReadOnlyDictionary(attr);

            // We can't hash anonymous objects from external assemblies, they must be merged in.
            if (type.Namespace != null)
                return hash;

            var result = new Dictionary<string, object>();
            foreach (var member in AccessorMembers.Create(type))
                result[member.Name] = accessor[attr, member.Name];

            return result;
        }
        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return Inner.TryGetValue(binder.Name, out result);
        }
    }
}