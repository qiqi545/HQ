using System.Data.Common;

namespace tophat
{
    public interface IConnectionFactory
    {
        string ConnectionString { get; set; }
        DbConnection CreateConnection();
        DbConnection GetUnitOfWorkScopedConnection();
        bool IsActive { get; }
        void Reset();
    }
}