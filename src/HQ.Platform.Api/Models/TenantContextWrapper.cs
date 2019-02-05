namespace HQ.Platform.Api.Models
{
    public class TenantContextWrapper<TTenant> : ITenantContext<TTenant> where TTenant : class, new()
    {
        public TTenant Value { get; }

        public TenantContextWrapper(TTenant tenant)
        {
            Value = tenant;
        }
    }
}
