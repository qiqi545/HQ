using HQ.Platform.Api.Controllers;

namespace HQ.Platform.Api.Models
{
    public interface IMetaProvider
    {
        void Populate(string baseUri, MetaCollection collection);
    }
}
