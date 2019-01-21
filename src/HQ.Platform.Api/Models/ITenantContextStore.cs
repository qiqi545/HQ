using System.Threading.Tasks;

namespace HQ.Platform.Api.Models
{
    public interface ITenantContextStore<TTenant> where TTenant : class
    {
        Task<TenantContext<TTenant>> FindByKeyAsync(string tenantKey);
    }
}
