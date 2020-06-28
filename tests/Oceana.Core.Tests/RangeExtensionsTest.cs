using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using Xunit;

namespace Oceana.Core.Tests
{
    public class RangeExtensionsTest
    {
        [Fact]
        public void ToListShouldReturnCorrectLength()
        {
            var result = (0..10).ToList();

            result.ShouldBe(new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        [Fact]
        public void ToListShouldStartAtCorrectNumber()
        {
            var result = (3..6).ToList();

            result.ShouldBe(new List<int> { 3, 4, 5, 6 });
        }

        [Fact]
        public void ToListShouldHandleSingleValueRange()
        {
            var result = (5..5).ToList();

            result.ShouldBe(new List<int> { 5 });
        }

        [Fact]
        public void ToListShouldHandleBackwardsRanges()
        {
            var result = (6..3).ToList();

            result.ShouldBe(new List<int> { 6, 5, 4, 3 });
        }
    }
}
