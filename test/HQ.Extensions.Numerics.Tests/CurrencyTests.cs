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

using System.Globalization;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Extensions.Numerics.Tests
{
    public class CurrencyTests : IClassFixture<CultureFixture>
    {
        private readonly ITestOutputHelper _console;

        public CurrencyTests(ITestOutputHelper console)
        {
            _console = console;
        }

        [Fact]
        public void Can_create_currency_using_culture_info()
        {
            CurrencyInfo currencyInfo = new CultureInfo("fr-FR");
            Assert.NotNull(currencyInfo);
        }

        [Fact]
        public void Can_create_currency_using_currency_code()
        {
            CurrencyInfo currencyInfo = Currency.NZD;
            Assert.NotNull(currencyInfo);
        }

        [Fact]
        public void Can_create_currency_using_current_culture()
        {
            CurrencyInfo currencyInfo = CultureInfo.CurrentCulture;
            Assert.NotNull(currencyInfo);
        }

        [Fact]
        public void Can_create_currency_using_region_info()
        {
            CurrencyInfo currencyInfo = new RegionInfo("CA");
            Assert.NotNull(currencyInfo);
        }

        [Fact]
        public void Currency_creation_creates_meaningful_display_cultures()
        {
            // If I'm from France, and I reference Canadian Dollars,
            // then the default culture for CAD should be fr-CA
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            CurrencyInfo currencyInfo = Currency.CAD;
            Assert.Equal(currencyInfo.DisplayCulture, new CultureInfo("fr-CA"));

            // If I'm from England, and I reference Canadian Dollars,
            // then the default culture for CAD should be en-CA
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            currencyInfo = Currency.CAD;
            Assert.Equal(currencyInfo.DisplayCulture, new CultureInfo("en-CA"));

            // If I'm from Germany, and I reference Canadian Dollars,
            // then the default culture for CAD should be Canadian
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            currencyInfo = Currency.CAD;
            Assert.Equal(new CultureInfo("en-CA"), currencyInfo.DisplayCulture);

            // ... and it should not display as if it were in DE currency!
            Money money = new Money(Currency.CAD, 1000);
            Assert.Equal("$1,000.00", money.DisplayNative());

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
            money = new Money(1000);
            var german = new CultureInfo("de-DE");
            _console.WriteLine(money.DisplayIn(german)); // Output: $1,000.00
        }

        [Fact]
        public void Currency_creation_creates_meaningful_native_regions()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            CurrencyInfo currencyInfo = Currency.EUR;
            Assert.Equal(currencyInfo.NativeRegion, new RegionInfo("FR"));

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-CA");
            currencyInfo = Currency.CAD;
            Assert.Equal(currencyInfo.NativeRegion, new RegionInfo("CA"));
        }
    }
}
