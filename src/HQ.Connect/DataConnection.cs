using System.Data;

namespace tophat
{
	public class DataConnection<T> : DataConnection, IDataConnection<T>
	{
		public DataConnection(DataContext current) : base(current) { }
	}

	public class DataConnection : IDataConnection
	{
		public IDbConnection Current => _current.Connection;

		private readonly DataContext _current;

		public DataConnection(DataContext current)
		{
			_current = current;
		}
	}
}