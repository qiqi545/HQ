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
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TypeKitchen;

namespace HQ.Integration.DocumentDb.DbProvider
{
	public static class Defaults
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

						var accessor = ReadAccessor.Create(instance.GetType());
						var value = accessor[instance, property.PropertyName];
						var @default = property.PropertyType.IsValueType
							? Activator.CreateInstance(property.PropertyType)
							: null;
						return value == null && @default == null || value != null && value.Equals(@default);
					};
				return property;
			}
		}
	}
}