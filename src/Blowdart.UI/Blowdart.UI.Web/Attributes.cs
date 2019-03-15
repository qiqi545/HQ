// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Dynamic;
using DotLiquid;

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

            return new Attributes(Hash.FromAnonymousObject(attr, true));
        }

        public static Attributes Attr(params object[] attr)
        {
            var hash = Hash.FromAnonymousObject(attr[0], true);
            for (var i = 1; i < attr.Length; i++)
                hash.Merge(Hash.FromAnonymousObject(attr[i]));
            return new Attributes(hash);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return Inner.TryGetValue(binder.Name, out result);
        }
    }
}