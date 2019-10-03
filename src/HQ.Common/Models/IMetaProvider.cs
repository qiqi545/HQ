using System;

namespace HQ.Common.Models
{
	public interface IMetaProvider
    {
        void Populate(string baseUri, MetaCollection collection, IServiceProvider serviceProvider);
    }
}
