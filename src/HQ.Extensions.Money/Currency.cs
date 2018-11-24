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

namespace HQ.Extensions.Money
{
    /// <summary>
    ///     Represents world currency by numeric and alphabetic values, as per ISO 4217:
    ///     http://www.iso.org/iso/currency_codes_list-1. This enum is implicitly converted
    ///     to <see cref="CurrencyInfo" /> instances internally, so you only need to reference this
    ///     enum to work with rich currency objects.
    /// </summary>
    [Serializable]
    public enum Currency : ushort
    {
        /// <summary>
        ///     USD
        /// </summary>
        USD = 840,

        /// <summary>
        ///     CAD
        /// </summary>
        CAD = 124,

        /// <summary>
        ///     EUR
        /// </summary>
        EUR = 978,

        /// <summary>
        ///     AUD
        /// </summary>
        AUD = 036,

        /// <summary>
        ///     GBP
        /// </summary>
        GBP = 826,

        /// <summary>
        ///     INR
        /// </summary>
        INR = 356,

        /// <summary>
        ///     JPY
        /// </summary>
        JPY = 392,

        /// <summary>
        ///     CHF
        /// </summary>
        CHF = 756,

        /// <summary>
        ///     NZD
        /// </summary>
        NZD = 554
    }
}
