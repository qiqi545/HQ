namespace HQ.Common.AspNetCore.Models
{
	public interface IMetaProvider
    {
        void Populate(string baseUri, MetaCollection collection);
    }
}
