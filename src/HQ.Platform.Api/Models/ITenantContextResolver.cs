using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HQ.Platform.Api.Models
{
    public interface ITenantContextResolver<TTenant> where TTenant : class
    {
        Task<TenantContext<TTenant>> ResolveAsync(HttpContext http);
    }
}
