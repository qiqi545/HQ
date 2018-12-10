using System.Collections.Generic;
using System.Linq;

namespace HQ.Rosetta.Queryable
{
    public class NoQueryableProvider<T> : IQueryableProvider<T>
    {
        public bool IsSafe => false;
        public bool SupportsUnsafe => false;
        public IQueryable<T> Queryable => null;
        public IQueryable<T> UnsafeQueryable => null;
        public ISafeQueryable<T> SafeQueryable => null;
        public IEnumerable<T> SafeAll => null;
    }
}
