using System;
using System.Linq;
using NUnit.Framework;

namespace Dates.Tests
{
    [TestFixture]
    public class PeriodTests
    {
        [Test]
        public void Can_get_occurrences()
        {
            var start = new DateTime(2009, 09, 01);

            var end = new DateTime(2010, 09, 01);

            var period = DatePeriod.Weekly;

            var occurrences = period.GetOccurrences(start, end);

            foreach (var occurrence in occurrences)
            {
                Console.WriteLine("{0}({1})", occurrence, occurrence.DayOfWeek);
            }

            Assert.AreEqual(52, occurrences.Count());
        }

        [Test]
        public void Can_get_occurrences_when_start_date_falls_on_a_weekend()
        {
            var start = new DateTime(2009, 09, 05);

            var end = new DateTime(2010, 09, 05);

            var period = DatePeriod.Weekly;

            var occurrences = period.GetOccurrences(start, end);

            foreach (var occurrence in occurrences)
            {
                Console.WriteLine("{0}({1})", occurrence, occurrence.DayOfWeek);
            }

            Assert.AreEqual(52, occurrences.Count());
        }
    }
}