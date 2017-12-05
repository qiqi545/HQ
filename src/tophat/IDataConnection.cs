using System.Data;

namespace tophat
{
	public interface IDataConnection
	{
		IDbConnection Current { get; }
	} 

	public interface IDataConnection<T> : IDataConnection { }
}