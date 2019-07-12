using System;
using System.Collections.Generic;

namespace HQ.Extensions.Metrics.SplitTesting
{
    public struct ExperimentKey : IComparable<ExperimentKey>
    {
        public string Name { get; }

        public ExperimentKey(string name) : this()
        {
            Name = name;
        }

        public bool Equals(ExperimentKey other)
        {
            return string.Equals(Name, other.Name);
        }

        public int CompareTo(ExperimentKey other)
        {
            return Name.CompareTo(other.CompareTo(other));
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) && (obj is ExperimentKey key && Equals(key));
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }

        public static bool operator ==(ExperimentKey left, ExperimentKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ExperimentKey left, ExperimentKey right)
        {
            return !left.Equals(right);
        }

        private sealed class NameEqualityComparer : IEqualityComparer<ExperimentKey>
        {
            public bool Equals(ExperimentKey x, ExperimentKey y)
            {
                return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(ExperimentKey obj)
            {
                return (obj.Name != null ? obj.Name.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<ExperimentKey> NameComparer { get; } = new NameEqualityComparer();
    }
}
