using System.Collections.Generic;

namespace HQ.Data.Contracts
{
    public interface IStream<out T> : IStreamHeader, IEnumerable<T> { }
}