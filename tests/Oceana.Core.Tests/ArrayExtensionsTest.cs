using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Shouldly;
using Xunit;

namespace Oceana.Core.Tests
{
    public class ArrayExtensionsTest
    {
        [Fact]
        [SuppressMessage("", "CS8604", Justification = "Testing for null reference")]
        public void CopyToShouldThrowExceptionWhenInputNull()
        {
            float[]? input = null;
            float[] output = new float[1];

            var result = Should.Throw<ArgumentNullException>(() => _ = input.CopyTo(output, 0, 1));

            result.ParamName.ShouldBe("input");
        }

        [Fact]
        [SuppressMessage("", "CS8604", Justification = "Testing for null reference")]
        public void CopyToShouldThrowExceptionWhenOutputNull()
        {
            float[] input = new float[1];
            float[]? output = null;

            var result = Should.Throw<ArgumentNullException>(() => _ = input.CopyTo(output, 0, 1));

            result.ParamName.ShouldBe("output");
        }

        [Fact]
        public void CopyToShouldReportLowerOfCountOrArrayLength()
        {
            float[] input = new float[1];
            float[] output = new float[2];

            var result = input.CopyTo(output, 0, 2);

            result.ShouldBe(1);
        }

        [Fact]
        public void CopyToShouldOnlyCopyUpToInputLengthIfLowerThanCount()
        {
            float[] input = new float[] { 1f };
            float[] output = new float[2];

            _ = input.CopyTo(output, 0, 2);

            output.SequenceEqual(new float[] { 1f, 0f }).ShouldBeTrue();
        }

        [Fact]
        public void CopyToShouldCopyFromOffset()
        {
            float[] input = new float[] { 1f };
            float[] output = new float[2];

            _ = input.CopyTo(output, 1, 1);

            output.SequenceEqual(new float[] { 0f, 1f }).ShouldBeTrue();
        }
    }
}
