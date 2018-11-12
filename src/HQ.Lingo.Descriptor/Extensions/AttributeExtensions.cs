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

namespace HQ.Lingo.Descriptor.Extensions
{
    internal static class AttributeExtensions
    {
        public static bool Has<T>(this PropertyAccessor accessor) where T : Attribute
        {
            Attribute exists = accessor.Attributes.OfType<T>().FirstOrDefault();
            return exists != null;
        }

        public static T Get<T>(this PropertyAccessor accessor) where T : Attribute
        {
            var exists = accessor.Attributes.OfType<T>().FirstOrDefault();
            return exists;
        }
    }
}
