using System;
using System.Collections.Generic;

namespace HQ.Common.Extensions
{
    internal struct PredicateEnumerator<T>
    {
        private readonly List<T> _inner;
        private readonly Predicate<T> _predicate;
        private int _index;

        public PredicateEnumerator(List<T> inner, Predicate<T> predicate)
        {
            _inner = inner;
            _predicate = predicate;
            _index = 0;
        }

        public T Current => _inner == null || _index == 0 ? default : _predicate(_inner[_index - 1]) ? _inner[_index - 1] : default;

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
