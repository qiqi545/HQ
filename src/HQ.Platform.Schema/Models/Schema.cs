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
using System.Runtime.Serialization;
using HQ.Platform.Schema.Extensions;

namespace HQ.Platform.Schema.Models
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplayName) + "}")]
    public class Schema : IEquatable<Schema>
    {
        public SchemaType Type { get; set; } = SchemaType.Object;
        public IList<Property> Properties { get; set; } = new List<Property>();
        public IList<string> Values { get; } = new List<string>();
        public string Description { get; set; }

        [NotMapped, IgnoreDataMember]
        public List<KeyValuePair<string, Schema>> Scope { get; set; } = new List<KeyValuePair<string, Schema>>();

        [NotMapped] private string DebuggerDisplayName => this.FullTypeString();

        public string Name { get; set; }
        public string Namespace { get; set; }

        public IEnumerable<KeyValuePair<string, Schema>> GetMap()
        {
            return Scope;
        }

        public bool Equals(Schema other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Type == other.Type && Equals(Properties, other.Properties) && Equals(Values, other.Values) && string.Equals(Description, other.Description) && string.Equals(Name, other.Name) && string.Equals(Namespace, other.Namespace);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Schema) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ (Properties != null ? Properties.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Values != null ? Values.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Schema left, Schema right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Schema left, Schema right)
        {
            return !Equals(left, right);
        }
    }
}
