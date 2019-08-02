using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HQ.Platform.Api.Models
{
	[DataContract]
	[KnownType(nameof(GetKnownTypes))]
	public class NestedBody<T> : Nested
	{
		private static IEnumerable<Type> GetKnownTypes() { return KnownTypesContext.GetKnownTypes(); }

		[DataMember] public T Data;
	}
}