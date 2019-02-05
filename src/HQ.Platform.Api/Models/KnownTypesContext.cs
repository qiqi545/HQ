using System;
using System.Collections.Generic;
using HQ.Data.Contracts;

namespace HQ.Platform.Api.Models
{
    public static class KnownTypesContext
    {
        public static Type[] KnownTypes { get; set; }

        public static IEnumerable<Type> GetKnownTypes()
        {
            yield return typeof(Error);
            yield return typeof(PagingInfo);
            foreach (var type in KnownTypes)
                yield return type;
        }
    }
}