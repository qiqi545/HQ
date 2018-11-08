using ImpromptuInterface;

namespace HQ.Touchstone.Assertions
{
	public static class ShouldExtensions
	{
		public static IShould<T> Should<T>(this T value) where T : class
		{
			return (new {Value = value}).ActLike<IShould<T>>();
		}
	}
}