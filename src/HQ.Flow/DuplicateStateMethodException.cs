using System;

namespace HQ.Flow
{
	[Serializable]
	public class DuplicateStateMethodException : Exception
	{
		public DuplicateStateMethodException(params string[] stateMethods) : base(
			"Duplicate state methods were found: \n" + string.Join("\n", stateMethods))
		{

		}
	}
}