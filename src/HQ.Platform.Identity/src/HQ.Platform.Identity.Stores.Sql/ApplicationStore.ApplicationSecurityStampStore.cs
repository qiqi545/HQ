using System.Threading;
using System.Threading.Tasks;
using HQ.Platform.Identity.Models;

namespace HQ.Platform.Identity.Stores.Sql
{
    public partial class ApplicationStore<TApplication, TKey> : IApplicationSecurityStampStore<TApplication>
    {
        public Task SetSecurityStampAsync(TApplication application, string stamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            application.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task<string> GetSecurityStampAsync(TApplication application, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(application?.SecurityStamp);
        }
    }
}
