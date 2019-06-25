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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace HQ.Platform.Schema.Models
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplayName) + "}")]
    public sealed class Property : IEquatable<Property>
    {
        public string Name { get; set; }
        public PropertyType Type { get; set; }
        public string From { get; set; }
        public PropertyRelationship Rel { get; set; } = PropertyRelationship.Scalar;
        public PropertyScope Scope { get; set; } = PropertyScope.ReadWrite;
        public string Default { get; set; }

        public bool Nullable { get; set; }
        public bool Required { get; set; }
        public bool Disabled { get; set; }
        public bool Protected { get; set; }
        public bool Personal { get; set; }

        public Dictionary<string, string> Annotations { get; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string Description { get; set; }

        [NotMapped] private string DebuggerDisplayName => $"{Name} ({Type})";

        [NotMapped]
        public bool IsComputed => Type.IsString() && From != null;

        public bool Equals(Property other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Name, other.Name) && Type == other.Type && string.Equals(From, other.From) &&
                   Rel == other.Rel && Scope == other.Scope && string.Equals(Default, other.Default) &&
                   Nullable == other.Nullable && Required == other.Required && Disabled == other.Disabled &&
                   Protected == other.Protected && Personal == other.Personal &&
                   Equals(Annotations, other.Annotations) && string.Equals(Description, other.Description);
        }

        public override bool Equals(object obj)
        {
            return !ReferenceEquals(null, obj) &&
                   (ReferenceEquals(this, obj) || obj.GetType() == this.GetType() && Equals((Property) obj));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Type;
                hashCode = (hashCode * 397) ^ (From != null ? From.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Rel;
                hashCode = (hashCode * 397) ^ (int) Scope;
                hashCode = (hashCode * 397) ^ (Default != null ? Default.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Nullable.GetHashCode();
                hashCode = (hashCode * 397) ^ Required.GetHashCode();
                hashCode = (hashCode * 397) ^ Disabled.GetHashCode();
                hashCode = (hashCode * 397) ^ Protected.GetHashCode();
                hashCode = (hashCode * 397) ^ Personal.GetHashCode();
                hashCode = (hashCode * 397) ^ (Annotations != null ? Annotations.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Property left, Property right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Property left, Property right)
        {
            return !Equals(left, right);
        }
    }
}
