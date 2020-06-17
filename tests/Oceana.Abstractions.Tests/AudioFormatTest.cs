using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using Xunit;

namespace Oceana.Abstractions.Tests
{
    public class AudioFormatTest
    {
        [Fact]
        public void EqualsOperatorShouldReturnCorrectly()
        {
            var left = new AudioFormat(1, 10);
            var right = new AudioFormat(1, 10);

            (left == right).ShouldBeTrue();
        }

        [Fact]
        public void EqualsShouldReturnFalseWhenNotAudioFormat()
        {
            new AudioFormat(1, 10).Equals(new object()).ShouldBeFalse();
        }

        [Fact]
        public void NotEqualOperatorShouldReturnCorrectlyForDifferingChannelWidth()
        {
            var left = new AudioFormat(1, 10);
            var right = new AudioFormat(2, 10);

            (left != right).ShouldBeTrue();
        }

        [Fact]
        public void NotEqualOperatorShouldReturnCorrectlyForDifferingSampleRate()
        {
            var left = new AudioFormat(1, 10);
            var right = new AudioFormat(1, 20);

            (left != right).ShouldBeTrue();
        }

        [Fact]
        public void GetHashCodeShouldReturnCorrectly()
        {
            new AudioFormat(1, 10).GetHashCode().ShouldBe(1 ^ 10);
        }
    }
}
