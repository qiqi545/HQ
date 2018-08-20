using System;

namespace HQ.Flow
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	internal class IgnoreStateMethodAttribute : Attribute { }
}