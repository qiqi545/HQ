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
using HQ.Data.Contracts;

namespace HQ.Data.Sql.Descriptor
{
    public class PropertyToColumn
    {
        public PropertyToColumn(PropertyAccessor typedPropertyDescriptor)
        {
            Property = typedPropertyDescriptor;
            ColumnName = Property.Name;
            Property.Name = Property.Name;
            IsTimestamp = typedPropertyDescriptor.Type == typeof(DateTimeOffset) &&
                          typedPropertyDescriptor.Name == nameof(IObject.CreatedAt);
        }

        public PropertyAccessor Property { get; }
        public string ColumnName { get; }
        public bool IsComputed { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsKey { get; set; }
        public bool IsTimestamp { get; }
    }
}
