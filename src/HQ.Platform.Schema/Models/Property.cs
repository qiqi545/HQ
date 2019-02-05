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
using System.Diagnostics;
using System.Runtime.Serialization;

namespace HQ.Platform.Schema.Models
{
    [DebuggerDisplay("{" + nameof(DebuggerDisplayName) + "}")]
    public class Property
    {
        public string Name { get; set; }
        public PropertyType Type { get; set; }
        public string From { get; set; }
        public PropertyRelationship Rel { get; set; } = PropertyRelationship.Scalar;
        public PropertyScope Scope { get; set; } = PropertyScope.ReadWrite;
        public string Default { get; set; }
        public bool Nullable { get; set; }
        public bool Required { get; set; }

        public Dictionary<string, string> Annotations { get; } =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string Description { get; set; }

        [IgnoreDataMember] private string DebuggerDisplayName => $"{Name} ({Type})";
    }
}
