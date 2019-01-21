namespace HQ.Platform.Api.Models
{
    public interface ITenantContext<out TTenant>
    {
        TTenant Value { get; }
    }
}
