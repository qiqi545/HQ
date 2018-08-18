using System;

namespace HQ.Flow
{
	[Serializable]
	public class AlreadyInitializedException : Exception
	{
		public AlreadyInitializedException() : base("StateProvider was already setup, and clear was not called before calling setup again")
		{
			
		}
	}
}