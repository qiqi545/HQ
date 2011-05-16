using System;

namespace Depot
{
    public interface ICacheDependency : IDisposable
    {
        string Id { get; }
        bool ShouldInvalidate { get; }
        DateTime LastModified { get; }
    }
}