namespace HQ.Domicile.Models
{
    public interface ITenant<TKey>
    {
        TKey Id { get; set; }
        string Name { get; set; }
    }
}
