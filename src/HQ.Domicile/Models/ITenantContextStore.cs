using System.Threading.Tasks;

namespace HQ.Domicile.Models
{
    public interface ITenantContextStore<TTenant> where TTenant : class
    {
        Task<TenantContext<TTenant>> FindByKeyAsync(string tenantKey);
    }
}
