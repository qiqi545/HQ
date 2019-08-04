using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using HQ.Data.Contracts.Mvc;

namespace HQ.Platform.Api.Models
{
	[DataContract]
	[KnownType(nameof(GetKnownTypes))]
	public class NestedCollectionBody<T> : Nested
	{
		private static IEnumerable<Type> GetKnownTypes() { return KnownTypesContext.GetKnownTypes(); }

		[DataMember] public IEnumerable<T> Data;
	}
}