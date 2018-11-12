using System;
using System.Collections.Generic;
using Dapper;
using table_descriptor;

namespace tuxedo.Dapper
{
    public static partial class TuxedoExtensions
    {
        private static void MapBackId<T>(IDescriptor descriptor, T entity, object id) where T : class
        {
            if (descriptor.Identity == null) return;
            if (descriptor.Identity.Property.Type != typeof(int) || id is int)
            {
                descriptor.Identity.Property.Set(entity, id);
            }
            else
            {
                descriptor.Identity.Property.Set(entity, Convert.ToInt32(id));
            }
        }

        private static DynamicParameters Prepare(this IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var result = new DynamicParameters();
            foreach(var parameter in parameters)
            {
                result.Add(parameter.Key, parameter.Value);
            }
            return result;
        }
    }
}
