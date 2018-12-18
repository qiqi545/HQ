// Copyright (c) HQ.IO. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using FastMember;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace System.Data.DocumentDb
{
    internal static class Defaults
    {
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            ContractResolver = new JsonContractResolver(),

            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.FFFFFF'Z'"
        };

        public static readonly Dictionary<string, Type> Metadata = new Dictionary<string, Type>
        {
            // UseTypeDiscrimination:
            {"DocumentType", typeof(string)},

            // Azure:
            {"id", typeof(string)},
            {"_rid", typeof(string)},
            {"_self", typeof(string)},
            {"_etag", typeof(string)},
            {"_attachments", typeof(string)},
            {"_ts", typeof(long)}
        };

        private class JsonContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (property.PropertyName == "Id")
                    property.ShouldDeserialize = instance =>
                    {
                        // WARNING:
                        // Azure inserts metadata into documents after all other properties, so this workaround
                        // will only opt-in to deserialize "Id" if it hasn't been set yet; this gets around
                        // the case where the mapped object's key is "Id", but it is a different type than "id"

                        var accessor = TypeAccessor.Create(instance.GetType());
                        var value = accessor[instance, property.PropertyName];
                        var @default = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;
                        return value == null && @default == null || value != null && value.Equals(@default);
                    };
                return property;
            }
        }
    }
}
