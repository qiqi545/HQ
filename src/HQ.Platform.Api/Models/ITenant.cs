namespace HQ.Platform.Api.Models
{
    public interface ITenant<TKey>
    {
        TKey Id { get; set; }
        string Name { get; set; }
    }
}
