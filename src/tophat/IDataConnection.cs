using System.Data;

namespace tophat
{
	public interface IDataConnection
	{
		IDbConnection Current { get; }
	} 

	public interface IDataConnection<T> : IDataConnection { }

	public class DataConnection<T> : IDataConnection<T>
	{
		private readonly DataContext _current;

		public DataConnection(DataContext current)
		{
			_current = current;
		}

		public IDbConnection Current => _current.Connection;
	}
}