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
using System.Collections;
using System.Collections.Generic;
using TypeKitchen;

namespace HQ.Extensions.Options
{
    public static class ConfigurationExtensions
    {
        public static Dictionary<string, string> Unbind(this object instance, string key)
        {
            var accessor = ReadAccessor.Create(instance);
            var map = new Dictionary<string, string>();
            foreach (var member in AccessorMembers.Create(instance, AccessorMemberScope.Public, AccessorMemberTypes.Properties))
            {
                var prefix = $"{key}:{member.Name}";
                if (!accessor.TryGetValue(instance, member.Name, out var value))
                    continue;

                var type = member.Type;

                if (type.IsValueTypeOrNullableValueType())
                {
                    map.Add(prefix, value?.ToString());
                }
                else if (value is IEnumerable enumerable)
                {
                    var concat = Pooling.StringBuilderPool.Scoped(sb =>
                    {
                        var count = 0;
                        foreach (var item in enumerable)
                        {
                            if (count != 0)
                                sb.Append(',');
                            sb.Append(item);
                            count++;
                        }
                    });
                    map.Add(prefix, concat);
                }
                else
                {
                    foreach (var child in value.Unbind(prefix))
                        map.Add(child.Key, child.Value);
                }
            }
            return map;
        }
    }
}
