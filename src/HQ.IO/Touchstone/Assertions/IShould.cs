namespace HQ.Touchstone.Assertions
{
	public interface IShould<out T> where T : class
	{
		T Value { get; }
	}
}