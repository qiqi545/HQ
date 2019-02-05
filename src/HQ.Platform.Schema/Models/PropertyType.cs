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

namespace HQ.Platform.Schema.Models
{
    public enum PropertyType : short
    {
        // Primitives:
        String,
        Boolean,
        Byte,
        Int32,
        Int16,
        Int64,
        Single,
        Double,
        Decimal,
        Date,
        DateTime,
        TimeSpan,

        // Special Types:
        Money,
        Email,
        Password,
        CreditCard,
        Phone,

        // Object Types:
        Object,
        View,
        Enum,

        // Aliases:
        O = Object,
        V = View,
        E = Enum,

        Bool = Boolean,
        Short = Int16,
        Integer = Int32,
        Int = Int32,
        Long = Int64,
        Float = Single,
        Currency = Money,
        PhoneNumber = Phone
    }
}
