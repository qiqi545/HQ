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

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using HQ.Platform.Schema.Extensions;

namespace HQ.Platform.Schema.Models
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplayName) + "}")]
    public class Schema : ISelfDescribingSchema
    {
        public Self Self { get; set; }
        public SchemaType Type { get; set; } = SchemaType.Object;
        public IList<Property> Properties { get; set; } = new List<Property>();
        public IList<string> Values { get; } = new List<string>();
        public string Description { get; set; }

        [IgnoreDataMember] public IDictionary<string, Schema> Scope { get; set; } = new Dictionary<string, Schema>();

        [IgnoreDataMember] private string DebuggerDisplayName => this.FullTypeString();

        public string Name { get; set; }

        #region ISelfDescribingJson

        public string Namespace => Self?.Namespace;

        public IDictionary<string, ISelfDescribingSchema> GetMap(string ns)
        {
            return Scope.ToDictionary(k => k.Key, v => v.Value as ISelfDescribingSchema);
        }

        #endregion
    }
}
