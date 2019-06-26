using System;
using System.Threading;
using System.Threading.Tasks;

namespace HQ.Data.Contracts.Components
{
    public interface IComponentStore<TKey, TObject> where TKey : IEquatable<TKey>
    {
        Task<Operation<ObjectSave>> CreateAsync(TObject role, CancellationToken cancellationToken);
    }
}
