using System.Collections.Generic;

namespace HQ.Common.Extensions
{
    internal struct SelfEnumerable<T>
    {
        private readonly List<T> _inner;
        
        public SelfEnumerable(List<T> inner)
        {
            _inner = inner;
        }

        public SelfEnumerator<T> GetEnumerator()
        {
            return new SelfEnumerator<T>(_inner);
        }
    }
}
