namespace HQ.Flow.Tests.States
{
	public class DerivedState : StateProvider.State
	{
		public virtual int Sprockets => 10;
		public virtual int Widgets => 5;
	}
}