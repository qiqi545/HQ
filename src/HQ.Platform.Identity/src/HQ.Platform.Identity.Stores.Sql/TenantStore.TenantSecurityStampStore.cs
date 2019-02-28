using System.Threading;
using System.Threading.Tasks;
using HQ.Platform.Identity.Models;

namespace HQ.Platform.Identity.Stores.Sql
{
    public partial class TenantStore<TTenant, TKey> : ITenantSecurityStampStore<TTenant>
    {
        public Task SetSecurityStampAsync(TTenant tenant, string stamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            tenant.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public Task<string> GetSecurityStampAsync(TTenant tenant, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(tenant?.SecurityStamp);
        }
    }
}
