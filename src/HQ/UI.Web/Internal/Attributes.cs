﻿#region LICENSE

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
using System.Dynamic;
using System.Linq;
using TypeKitchen;

namespace HQ.UI.Web.Internal
{
	internal class Attributes : DynamicObject
	{
		internal static object[] NoAttributes = new object[0];
		internal static Attributes Empty = new Attributes();
		internal static IReadOnlyDictionary<string, object> EmptyHash = new Dictionary<string, object>();

		private static readonly object Sync = new object();

		private Attributes(IReadOnlyDictionary<string, object> inner) => Inner = inner;

		private Attributes() { }

		internal IReadOnlyDictionary<string, object> Inner { get; }

		public static Attributes Attr(object attr)
		{
			if (attr == NoAttributes)
				return Empty;

			if (attr is string)
				throw new HtmlException(
					$"You provided a string literal for an attribute object. Did you mean new {{ @class = \"{attr}\" }} ?");

			return new Attributes(CreateHash(attr));
		}

		public static Attributes Attr(params object[] attr)
		{
			if (attr.Length == 0)
				return Empty;

			var instance = attr[0];
			var hash = instance is Attributes toMerge
				? toMerge.Inner
				: CreateAccessor(instance.GetType()).AsReadOnlyDictionary(instance);
			for (var i = 1; i < attr.Length; i++)
				switch (attr[i])
				{
					case string _:
						throw new HtmlException(
							$"You provided a string literal for an attribute object. Did you mean new {{ @class = \"{attr[i]}\" }} ?");
					case Attributes other:
						hash = hash.Concat(other.Inner).ToDictionary(k => k.Key, v => v.Value); // TODO no
						break;
					default:
						hash = hash.Concat(CreateHash(attr[i])).ToDictionary(k => k.Key, v => v.Value); // TODO no
						break;
				}

			return new Attributes(hash);
		}

		private static IReadOnlyDictionary<string, object> CreateHash(object attr)
		{
			if (attr == null)
				return EmptyHash;

			var accessorType = attr.GetType();
			var accessor = CreateAccessor(accessorType);

			if (!accessorType.IsAnonymous())
				return accessor.AsReadOnlyDictionary(attr);

			// We can't hash anonymous objects from external assemblies, they must be merged in.
			// TODO custom iterator, don't allocate
			var result = new Dictionary<string, object>();
			var members =
				AccessorMembers.Create(accessorType, AccessorMemberScope.Public, AccessorMemberTypes.Properties);
			foreach (var member in members)
			{
				if (!accessor.TryGetValue(attr, member.Name, out var value))
					continue; // unlikely, but defend against missing members in the accessor

				if (value == null)
					continue; // omit null values in the final hash

				if (value is bool flag)
				{
					if (!flag)
						continue;
					value = "true";
				}

				result[member.Name] = value;
			}

			return result;
		}

		private static ITypeReadAccessor CreateAccessor(Type type)
		{
			lock (Sync) return ReadAccessor.Create(type);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			return Inner.TryGetValue(binder.Name, out result);
		}
	}
}