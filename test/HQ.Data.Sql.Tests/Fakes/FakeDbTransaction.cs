using System.Data;
using HQ.Data.Sql.Queries;

namespace HQ.Data.Sql.Tests.Fakes
{
    public class FakeDbTransaction : IDbTransaction
    {
        public void Dispose() { }
        public void Commit() { }
        public void Rollback() { }

        public IDbConnection Connection => new FakeDbConnection(SqlBuilder.Dialect);
        public IsolationLevel IsolationLevel => IsolationLevel.Chaos;
    }
}
