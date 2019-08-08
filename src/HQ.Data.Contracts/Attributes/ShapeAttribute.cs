using System;

namespace HQ.Data.Contracts.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
	public class ShapeAttribute : Attribute
	{
		public Shape Shape { get; set; }

		public ShapeAttribute(Shape shape)
		{
			Shape = shape;
		}

		public string Name => Enum.GetName(typeof(Shape), (byte) Shape);
	}
}
