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
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TypeKitchen;

namespace HQ.Common.Serialization
{
	public class JsonContractResolver : DefaultContractResolver
	{
		private readonly JsonProcessingDirection _direction;
		private readonly ITextTransform _transform;

		public JsonContractResolver(ITextTransform transform, JsonProcessingDirection direction)
		{
			_transform = transform;
			_direction = direction;
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			if (IsCollection(property))
			{
				property.ShouldSerialize = instance => IsNotEmpty(member, instance);
			}

			return property;
		}

		private static bool IsNotEmpty(MemberInfo member, object instance)
		{
			var accessor = ReadAccessor.Create(instance.GetType());

			IEnumerable enumerable = null;
			switch (member.MemberType)
			{
				case MemberTypes.Property:
				case MemberTypes.Field:
					enumerable = accessor[instance, member.Name] as IEnumerable;
					break;
				case MemberTypes.All:
				case MemberTypes.Constructor:
				case MemberTypes.Custom:
				case MemberTypes.Event:
				case MemberTypes.Method:
				case MemberTypes.NestedType:
				case MemberTypes.TypeInfo:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return enumerable == null || enumerable.GetEnumerator().MoveNext();
		}

		private static bool IsCollection(JsonProperty property)
		{
			return property.PropertyType != typeof(string) &&
			       typeof(IEnumerable).IsAssignableFrom(property.PropertyType);
		}

		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			IList<JsonProperty> properties = new List<JsonProperty>();
			switch (_direction)
			{
				case JsonProcessingDirection.Input:
					properties = base.CreateProperties(type, memberSerialization);
					break;
				case JsonProcessingDirection.Output:
					IEnumerable<Type> interfaces =
						type.GetInterfaces().Where(i => typeof(IViewModel).IsAssignableFrom(i)).ToList();
					if (!interfaces.Any())
					{
						properties = base.CreateProperties(type, memberSerialization);
					}
					else
					{
						foreach (var interfaceType in interfaces)
						{
							var props = base.CreateProperties(interfaceType, memberSerialization);
							foreach (var prop in props)
							{
								if (!properties.Contains(prop))
								{
									properties.Add(prop);
								}
							}
						}
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return properties.OrderBy(p => p.PropertyName).ToList();
		}

		protected override string ResolvePropertyName(string propertyName)
		{
			return _transform == null ? base.ResolvePropertyName(propertyName) : _transform.Transform(propertyName);
		}
	}
}