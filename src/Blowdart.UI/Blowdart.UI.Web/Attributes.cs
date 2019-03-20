// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using DotLiquid;
using FastMember;

namespace Blowdart.UI.Web
{
    public class Attributes : DynamicObject
    {
        internal Hash Inner { get; }

        private Attributes(Hash inner)
        {
            Inner = inner;
        }

        public static Attributes Attr(object attr)
        {
            if(attr is string)
                throw new HtmlException($"You provided a string literal for an attribute object. Did you mean new {{ @class = \"{attr}\" }} ?");

            var hash = CreateHash(attr);

            return new Attributes(hash);
        }

        public static Attributes Attr(params object[] attr)
        {
            var hash = Hash.FromAnonymousObject(attr[0], true);
            for (var i = 1; i < attr.Length; i++)
            {
                switch (attr[i])
                {
                    case string _:
                        throw new HtmlException($"You provided a string literal for an attribute object. Did you mean new {{ @class = \"{attr[i]}\" }} ?");
                    case Attributes other:
                        hash.Merge(other.Inner);
                        break;
                    default:
                        hash.Merge(CreateHash(attr[i]));
                        break;
                }
            }
            return new Attributes(hash);
        }

        private static Hash CreateHash(object attr)
        {
            var type = attr.GetType();

            // We can't hash anonymous objects from external assemblies, they must be merged in.
            if (type.Namespace != null)
                return Hash.FromAnonymousObject(attr, true);

            var result = new Dictionary<string, object>();
            var accessor = TypeAccessor.Create(type);
            foreach (var member in accessor.GetMembers())
                result[member.Name] = accessor[attr, member.Name];

            return Hash.FromDictionary(result);
        }
        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return Inner.TryGetValue(binder.Name, out result);
        }
    }
}