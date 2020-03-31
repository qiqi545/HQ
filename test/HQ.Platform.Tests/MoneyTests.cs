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
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using HQ.Common;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Platform.Tests
{
	public class MoneyTests
	{
		private readonly ITestOutputHelper _console;

		public MoneyTests(ITestOutputHelper console)
		{
			_console = console;
		}

		[Fact]
		public void Can_add_money()
		{
			const double left = 10.00;
			const double right = 20.00;

			Money total = left + right;
			Assert.Equal(30.00m, (decimal) total);
		}

		[Fact]
		public void Can_create_money_by_decimals()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			Money money = 10.00;
			Assert.Equal(10.00, (double) money);
		}

		[Fact]
		public void Can_create_money_by_units()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			Money money = 1000;
			Assert.Equal(1000.00, (double) money);
		}

		[Fact]
		public void Can_create_money_in_current_currency()
		{
			Money_with_current_culture_has_correct_currency_code("fr-FR", Currency.EUR);
			Money_with_current_culture_has_correct_currency_code("en-US", Currency.USD);
			Money_with_current_culture_has_correct_currency_code("en-CA", Currency.CAD);
			Money_with_current_culture_has_correct_currency_code("en-AU", Currency.AUD);
			Money_with_current_culture_has_correct_currency_code("en-GB", Currency.GBP);
			Money_with_current_culture_has_correct_currency_code("ja-JP", Currency.JPY);

			// Subsequent tests rely on en-US culture for currency rules
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
		}

		private void Money_with_current_culture_has_correct_currency_code(string cultureName, Currency currency)
		{
			if (!Enum.IsDefined(typeof(Currency), currency))
				throw new InvalidEnumArgumentException(nameof(currency), (int) currency, typeof(Currency));

			Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
			Money money = 1000;
			Assert.Equal(currency, money.CurrencyInfo.Code);

			_console.WriteLine(money.CurrencyInfo.Code.ToString());
			_console.WriteLine(money.CurrencyInfo.DisplayCulture.ToString());
			_console.WriteLine(money.CurrencyInfo.DisplayName);
			_console.WriteLine(money.CurrencyInfo.NativeRegion.ToString());
		}

		[Fact]
		public void Can_determine_equality()
		{
			Money left = 456;
			Money right = 0.456;

			Assert.False(left == right);
			Assert.False(left.Equals(right));
			Assert.False((long) left == (long) right);
			Assert.False(left == (long) right);
			Assert.False((long) left == right);
		}

		[Fact]
		public void Can_display_currency_in_given_culture_preserving_native_culture_info()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
			Money expectedMoney = 1000;
			var expected = expectedMoney.ToString();
			_console.WriteLine(expected);

			// Display the fr-FR money in en-CA format
			var actual = expectedMoney.DisplayIn(new CultureInfo("en-CA"));
			_console.WriteLine(actual);
			Assert.NotEqual(expected, actual);
		}

		[Fact]
		public void Can_display_currency_in_given_culture_with_disambiguation()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
			Money expectedMoney = 1000;
			var expected = expectedMoney.ToString();
			_console.WriteLine(expected);

			// Display the en-US money in en-CA format with "USD" disambiguation
			var actual = expectedMoney.DisplayIn(new CultureInfo("en-CA"));
			_console.WriteLine(actual);
			Assert.NotEqual(expected, actual);
		}

		[Fact]
		public void Can_display_proper_culture_when_created_in_different_culture()
		{
			Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
			var money = new Money(Currency.CAD, 1000);
			Assert.Equal(new CultureInfo("fr-CA"), money.CurrencyInfo.DisplayCulture);
			_console.WriteLine(money.ToString());
		}

		[Fact]
		public void Can_divide_money()
		{
			const double left = 20.00;
			const double right = 2.00;

			Money total = left / right;
			Assert.Equal(10.00m, (decimal) total);
		}

		[Fact]
		public void Can_divide_money_with_decimals()
		{
			const decimal left = 20.00m;
			const decimal right = 2.00m;

			Money total = left / right;
			Assert.Equal(10.00m, (decimal) total);
		}

		[Fact]
		public void Can_divide_money_by_negative_identity()
		{
			var left = new Money(1);
			var right = new Money(-1);

			var total = right / left;
			Assert.Equal(-1, (decimal) total);
		}

		[Fact]
		public void Can_divide_money_by_negative_identity_with_decimals()
		{
			var left = new Money(1m);
			var right = new Money(-1m);

			var total = right / left;
			Assert.Equal(-1m, (decimal) total);
		}

		[Fact]
		public void Can_divide_money_by_positive_identity()
		{
			var left = new Money(1);
			var right = new Money(1);

			var total = right / left;
			Assert.Equal(1, (decimal) total);
		}

		[Fact]
		public void Can_divide_money_by_positive_identity_with_decimals()
		{
			var left = new Money(1m);
			var right = new Money(1m);

			var total = right / left;
			Assert.Equal(1m, (decimal) total);
		}

		[Fact]
		public void Can_handle_division_without_precision_loss()
		{
			Money left = 45;
			Money right = 13;

			var total = left / right; // 3.461538461538462

			Assert.Equal(3.46m, (decimal) total);
		}

		[Fact]
		public void Can_handle_division_without_precision_loss_with_decimals()
		{
			Money left = 45m;
			Money right = 13m;

			var total = left / right; // 3.461538461538462

			Assert.Equal(3.46m, (decimal) total);
		}

		[Fact]
		public void Can_handle_small_fractions()
		{
			Money total = 0.1;
			Assert.Equal(0.10m, (decimal) total);
		}

		[Fact]
		public void Can_handle_small_fractions_with_decimals()
		{
			Money total = 0.1m;
			Assert.Equal(0.10m, (decimal) total);
		}

		[Fact]
		public void Can_multiply_identity_without_casting()
		{
			var left = new Money(1.00);
			var right = new Money(1.00);

			var total = right * left;
			Assert.Equal(1.00m, (decimal) total);
		}

		[Fact]
		public void Can_multiply_identity_without_casting_with_decimals()
		{
			var left = new Money(1.00m);
			var right = new Money(1.00m);

			var total = right * left;
			Assert.Equal(1.00m, (decimal) total);
		}

		[Fact]
		public void Can_multiply_money()
		{
			var left = new Money(10.00);
			var right = new Money(20.00);

			var total = right * left;
			Assert.Equal(200.00m, (decimal) total);
		}

		[Fact]
		public void Can_multiply_money_with_decimals()
		{
			var left = new Money(10.00m);
			var right = new Money(20.00m);

			var total = right * left;
			Assert.Equal(200.00m, (decimal) total);
		}

		[Fact]
		public void Can_multiply_money_by_negative_identity()
		{
			const double left = 1.00;
			const double right = -1;

			Money total = right * left;
			Assert.Equal(-1, (decimal) total);
		}

		[Fact]
		public void Can_multiply_money_by_positive_identity()
		{
			const double left = 1.00;
			const double right = 1;

			Money total = right * left;
			Assert.Equal(1, (decimal) total);
		}

		[Fact]
		public void Can_multiply_non_identity_without_casting()
		{
			var left = new Money(4.00);
			var right = new Money(4.00);

			var total = right * left;
			Assert.Equal(16.00m, (decimal) total);
		}

		[Fact]
		public void Can_preserve_internal_precision()
		{
			Money total = 0.335678 * 345; // 115.80891

			// Loss of precision based on rounding rules
			Assert.Equal(115.81m, (decimal) total);

			// Adding .005 to 115.81 would equal 115.82 
			// due to rounding if precision was lost
			total += 0.005; // 115.81391

			Assert.Equal(115.81m, (decimal) total);
		}

		[Fact]
		public void Can_preserve_internal_precision_with_decimals()
		{
			Money total = 0.335678m * 345m; // 115.80891

			// Loss of precision based on rounding rules
			Assert.Equal(115.81m, (decimal) total);

			// Adding .005 to 115.81 would equal 115.82 
			// due to rounding if precision was lost
			total += 0.005m; // 115.81391

			Assert.Equal(115.81m, (decimal) total);
		}

		[Fact]
		public void Can_preserve_internal_rounding_against_larger_fractions()
		{
			Money total = 0.335678 * 345; // 115.80891

			// Loss of precision based on rounding rules
			Assert.Equal(115.81m, (decimal) total);

			// This number has greater precision than the original
			total += .00082809; // 115.80973809

			Assert.Equal(115.81m, (decimal) total);
		}

		[Fact]
		public void Can_preserve_internal_rounding_against_larger_fractions_with_decimals()
		{
			Money total = 0.335678m * 345m; // 115.80891

			// Loss of precision based on rounding rules
			Assert.Equal(115.81m, (decimal) total);

			// This number has greater precision than the original
			total += .00082809m; // 115.80973809

			Assert.Equal(115.81m, (decimal) total);
		}

		[Fact]
		public void Can_preserve_internal_rounding_against_smaller_fractions()
		{
			Money total = 0.335678 * 345; // 115.80891

			// Loss of precision based on rounding rules
			Assert.Equal(115.81m, (decimal) total);

			// This number has lesser precision than the original
			total += .456; // 116.26491

			Assert.Equal(116.26m, (decimal) total);
		}

		[Fact]
		public void Can_preserve_internal_rounding_against_smaller_fractions_with_decimals()
		{
			Money total = 0.335678m * 345m; // 115.80891

			// Loss of precision based on rounding rules
			Assert.Equal(115.81m, (decimal) total);

			// This number has lesser precision than the original
			total += .456m; // 116.26491

			Assert.Equal(116.26m, (decimal) total);
		}

		[Fact]
		public void Can_subtract_money()
		{
			const double left = 10.00;
			const double right = 20.00;

			Money total = right - left;
			Assert.Equal(10.00m, (decimal) total);
		}

		[Fact]
		public void Cannot_add_different_currencies()
		{
			var left = new Money(Currency.CAD, 10.00);
			var right = new Money(Currency.USD, 20.00);

			Assert.Throws<ArithmeticException>(() =>
			{
				var total = left + right;
				_console.WriteLine(total.ToString());
			});
		}
	}
}
