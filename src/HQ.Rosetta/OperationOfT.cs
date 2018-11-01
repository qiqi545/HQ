using System.Collections.Generic;

namespace HQ.Rosetta
{
	public sealed class Operation<T> : Operation
	{
		public T Data { get; set; }

		public Operation(T data) : this(data, null) { }

		public Operation(T data, IEnumerable<Error> errors) : base(errors)
		{
			Data = data;
		}
	}
}
