using System;
using System.Collections.Generic;

namespace HQ.Common.Extensions
{
    internal struct PredicateEnumerable<T>
    {
        private readonly List<T> _inner;
        private readonly Predicate<T> _predicate;

        public PredicateEnumerable(List<T> inner, Predicate<T> predicate)
        {
            _inner = inner;
            _predicate = predicate;
        }

        public PredicateEnumerator<T> GetEnumerator()
        {
            return new PredicateEnumerator<T>(_inner, _predicate);
        }
    }
}
