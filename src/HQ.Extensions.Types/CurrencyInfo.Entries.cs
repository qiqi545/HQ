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

namespace HQ.Extensions.Types
{
    partial class CurrencyInfo
    {
        private static readonly IDictionary<Currency, CurrencyInfo> _currencies
            = new Dictionary<Currency, CurrencyInfo>(2)
            {
                {
                    Currency.USD,
                    new CurrencyInfo
                    {
                        DisplayName = "US Dollar",
                        Code = Currency.USD
                    }
                },
                {
                    Currency.CAD,
                    new CurrencyInfo
                    {
                        DisplayName = "Canadian Dollar",
                        Code = Currency.CAD
                    }
                },
                {
                    Currency.EUR,
                    new CurrencyInfo
                    {
                        DisplayName = "Euro",
                        Code = Currency.EUR
                    }
                },
                {
                    Currency.GBP,
                    new CurrencyInfo
                    {
                        DisplayName = "Pound Sterling",
                        Code = Currency.GBP
                    }
                },
                {
                    Currency.JPY,
                    new CurrencyInfo
                    {
                        DisplayName = "Yen",
                        Code = Currency.JPY
                    }
                },
                {
                    Currency.CHF,
                    new CurrencyInfo
                    {
                        DisplayName = "Swiss Franc",
                        Code = Currency.CHF
                    }
                },
                {
                    Currency.AUD,
                    new CurrencyInfo
                    {
                        DisplayName = "Australian Dollar",
                        Code = Currency.AUD
                    }
                },
                {
                    Currency.NZD,
                    new CurrencyInfo
                    {
                        DisplayName = "New Zealand Dollar",
                        Code = Currency.NZD
                    }
                },
                {
                    Currency.INR,
                    new CurrencyInfo
                    {
                        DisplayName = "Indian Rupee",
                        Code = Currency.INR
                    }
                }
            };
    }
}
