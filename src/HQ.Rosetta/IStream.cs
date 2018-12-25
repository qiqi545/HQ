using System.Collections.Generic;

namespace HQ.Rosetta
{
    public interface IStream<out T> : IStreamHeader, IEnumerable<T> { }
}