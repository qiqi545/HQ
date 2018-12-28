using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using HQ.Domicile.Extensions;
using HQ.Rosetta;

namespace HQ.Domicile.Models
{
    public static class KnownTypesContext
    {
        public static Type[] KnownTypes { get; set; }

        public static IEnumerable<Type> GetKnownTypes()
        {
            yield return typeof(Error);
            yield return typeof(EnrichmentExtensions.PagingInfo);
            foreach (var type in KnownTypes)
                yield return type;
        }
    }

    [DataContract]
    [KnownType(nameof(GetKnownTypes))]
    public struct NestedBody
    {
        private static IEnumerable<Type> GetKnownTypes() => KnownTypesContext.GetKnownTypes();

        [DataMember]
        public object Data;

        [DataMember]
        public IList<Error> Errors;

        [DataMember]
        public bool HasErrors;
    }
}
