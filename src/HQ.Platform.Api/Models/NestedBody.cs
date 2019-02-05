using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using HQ.Data.Contracts;

namespace HQ.Platform.Api.Models
{
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
