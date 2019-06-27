using System.Data;

namespace HQ.Data.Sql.Tests.Fakes
{
    public class FakeDbTransaction : IDbTransaction
    {
        public void Dispose() { }
        public void Commit() { }
        public void Rollback() { }

        public IDbConnection Connection => new FakeDbConnection();
        public IsolationLevel IsolationLevel => IsolationLevel.Chaos;
    }
}
