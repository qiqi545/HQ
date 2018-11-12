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
using Dapper;

namespace HQ.Lingo.Dapper
{
    // Map back all computed columns

    public static partial class TuxedoExtensions
    {
        private static void MapBackId<T>(Descriptor.TableDescriptor.Descriptor descriptor, T entity, object id)
            where T : class
        {
            if (descriptor.Identity == null) return;
            if (descriptor.Identity.Property.Type != typeof(int) || id is int)
                descriptor.Identity.Property.Set(entity, id);
            else
                descriptor.Identity.Property.Set(entity, Convert.ToInt32(id));
        }

        private static DynamicParameters Prepare(this IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var result = new DynamicParameters();
            foreach (var parameter in parameters) result.Add(parameter.Key, parameter.Value);
            return result;
        }
    }
}
