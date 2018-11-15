using System.Collections.Generic;

namespace HQ.Common.Extensions
{
    internal struct SelfEnumerator<T>
    {
        private readonly List<T> _inner;
        private int _index;

        public SelfEnumerator(List<T> inner)
        {
            _inner = inner;
            _index = 0;
        }

        public T Current => _inner == null || _index == 0 ? default : _inner[_index - 1];

        public bool MoveNext()
        {
            _index++;
            return _inner != null && _inner.Count >= _index;
        }

        public void Reset()
        {
            _index = 0;
        }
    }
}
