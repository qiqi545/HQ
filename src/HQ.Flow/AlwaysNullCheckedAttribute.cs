using System;

namespace HQ.Flow
{
	[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class AlwaysNullCheckedAttribute : Attribute
	{
	}
}