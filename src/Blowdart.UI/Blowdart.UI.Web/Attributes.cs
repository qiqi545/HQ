#region LICENSE
// Unless explicitly acquired and licensed from HQ.IO Corporation 
// under another license, the contents of this file are copyrighted, 
// and you are strictly prohibited from using it for any purpose. 
// Violations will be prosecuted under the full extent the law allows.
#endregion

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
            return new Attributes(Hash.FromAnonymousObject(attr, true));
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return Inner.TryGetValue(binder.Name, out result);
        }
    }
}