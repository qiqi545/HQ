using System.Data.Common;

namespace tophat
{
    public abstract class ConnectionFactory : IConnectionFactory
    {
        public abstract DbConnection CreateConnection();

        public DbConnection GetUnitOfWorkScopedConnection()
        {
            var context = GetContext();
            return context ? .Connection;
        }

        public void Reset()
        {
            var context = GetContext();
            context.Dispose();
        }

        private static DataContext GetContext()
        {
            var context = Database.Container.Resolve<DataContext>();
            return context;
        }

        public bool IsActive => GetContext().IsActive;

        public string ConnectionString { get; set; }
    }
}