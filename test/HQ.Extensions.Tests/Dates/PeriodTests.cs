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
using System.Linq;
using HQ.Extensions.Dates;
using Xunit;
using Xunit.Abstractions;

namespace HQ.Extensions.Tests.Dates
{
    public class PeriodTests
    {
        private readonly ITestOutputHelper _console;

        public PeriodTests(ITestOutputHelper console)
        {
            _console = console;
        }

        [Fact]
        public void Can_get_occurrences()
        {
            var start = new DateTime(2009, 09, 01);

            var end = new DateTime(2010, 09, 01);

            var period = DatePeriod.Weekly;

            var occurrences = period.GetOccurrences(start, end);

            foreach (var occurrence in occurrences)
            {
                _console.WriteLine("{0}({1})", occurrence, occurrence.DayOfWeek);
            }

            Assert.Equal(52, occurrences.Count());
        }

        [Fact]
        public void Can_get_occurrences_when_start_date_falls_on_a_weekend()
        {
            var start = new DateTime(2009, 09, 05);

            var end = new DateTime(2010, 09, 05);

            var period = DatePeriod.Weekly;

            var occurrences = period.GetOccurrences(start, end);

            foreach (var occurrence in occurrences)
            {
                _console.WriteLine("{0}({1})", occurrence, occurrence.DayOfWeek);
            }

            Assert.Equal(52, occurrences.Count());
        }
    }
}
