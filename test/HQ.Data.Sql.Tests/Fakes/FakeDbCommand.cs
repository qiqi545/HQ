using System.Data;

namespace HQ.Data.Sql.Tests.Fakes
{
    public class FakeDbCommand : IDbCommand
    {
        public void Dispose() { }

        public void Cancel() { }

        public IDbDataParameter CreateParameter()
        {
            return new FakeDbParameter();
        }

        public int ExecuteNonQuery()
        {
            return 1;
        }

        public IDataReader ExecuteReader()
        {
            return new FakeDataReader();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return new FakeDataReader();
        }

        public object ExecuteScalar()
        {
            return 1;
        }

        public void Prepare() { }

        public string CommandText { get; set; }
        public int CommandTimeout { get; set; }
        public CommandType CommandType { get; set; }
        public IDbConnection Connection { get; set; }
        public IDataParameterCollection Parameters { get; } = new FakeDbParameterCollection();
        public IDbTransaction Transaction { get; set; }
        public UpdateRowSource UpdatedRowSource { get; set; }
    }
}
