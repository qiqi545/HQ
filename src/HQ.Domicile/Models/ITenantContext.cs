namespace HQ.Domicile.Models
{
    public interface ITenantContext<out TTenant>
    {
        TTenant Value { get; }
    }
}
