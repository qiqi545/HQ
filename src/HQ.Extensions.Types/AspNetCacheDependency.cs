using System;
using System.Web.Caching;

namespace Depot.AspNet
{
    public class AspNetCacheDependency : ICacheDependency
    {
        public CacheDependency Internal { get; private set; }

        public AspNetCacheDependency(string filename)
        {
            Internal = new CacheDependency(filename);
        }

        public AspNetCacheDependency(string filename, DateTime start)
        {
            Internal = new CacheDependency(filename, start);
        }

        public AspNetCacheDependency(string[] filenames)
        {
            Internal = new CacheDependency(filenames);
        }

        public void Dispose()
        {
            Internal.Dispose();
        }
        
        public string Id
        {
            get { return Internal.GetUniqueID(); }
        }

        public bool ShouldInvalidate
        {
            get { return Internal.HasChanged; }
        }

        public DateTime LastModified
        {
            get { return Internal.UtcLastModified; }
        }
    }
}