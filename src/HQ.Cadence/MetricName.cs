#region LICENSE

// Unless explicitly acquired and licensed from Licensor under another
// license, the contents of this file are subject to the Reciprocal Public
// License ("RPL") Version 1.5, or subsequent versions as allowed by the RPL,
// and You may not copy or use this file in either source code or executable
// form, except in compliance with the terms and conditions of the RPL.
// 
// All software distributed under the RPL is provided strictly on an "AS
// IS" basis, WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESS OR IMPLIED, AND
// LICENSOR HEREBY DISCLAIMS ALL SUCH WARRANTIES, INCLUDING WITHOUT
// LIMITATION, ANY WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, QUIET ENJOYMENT, OR NON-INFRINGEMENT. See the RPL for specific
// language governing rights and limitations under the RPL.

#endregion

using System;

namespace HQ.Cadence
{
    /// <summary>
    ///     A hash key for storing metrics associated by the parent class and name pair
    /// </summary>
    public struct MetricName
    {
        public Type Class { get; }

        public string Name { get; }

        public MetricName(Type @class, string name) : this()
        {
            Class = @class;
            Name = name;
        }

        public bool Equals(MetricName other)
        {
            return Equals(other.Name, Name) && other.Class == Class;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is MetricName name && Equals(name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Class != null ? Class.GetHashCode() : 0);
            }
        }

        public static bool operator ==(MetricName left, MetricName right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MetricName left, MetricName right)
        {
            return !left.Equals(right);
        }
    }
}
