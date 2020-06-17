using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shouldly;
using Xunit;

namespace Oceana.Core.Tests
{
    public class CircularBufferTest
    {
        [Fact]
        public void ReadShouldReadAllDataIfItemsAvailable()
        {
            var buffer = new CircularBuffer<int>(10);
            var data = new int[8];

            _ = buffer.Write(data);
            var result = buffer.Read(4);

            result.Length.ShouldBe(4);
        }

        [Fact]
        public void ReadShouldReadPartialDataIfAllItemsNotAvailable()
        {
            var buffer = new CircularBuffer<int>(10);
            var data = new int[4];

            _ = buffer.Write(data);
            var result = buffer.Read(8);

            result.Length.ShouldBe(4);
        }

        [Fact]
        public void ReadShouldReadCorrectDataNonBoundry()
        {
            var buffer = new CircularBuffer<int>(10);
            var data = Enumerable.Range(1, 8).ToArray();

            _ = buffer.Write(data);
            var result = buffer.Read(4);

            result.SequenceEqual(Enumerable.Range(1, 4)).ShouldBeTrue();
        }

        [Fact]
        public void ReadShouldReadCorrectDataAroundBoundry()
        {
            var buffer = new CircularBuffer<int>(10);

            _ = buffer.Write(Enumerable.Range(1, 10).ToArray());
            _ = buffer.Read(8);
            _ = buffer.Write(Enumerable.Range(11, 5).ToArray());
            var result = buffer.Read(4);

            result.SequenceEqual(Enumerable.Range(9, 4)).ShouldBeTrue();
        }

        [Fact]
        public void ReadShouldReadAroundBoundry()
        {
            var buffer = new CircularBuffer<int>(10);

            _ = buffer.Write(new int[10]);
            _ = buffer.Read(8);
            _ = buffer.Write(new int[5]);
            var result = buffer.Read(4);

            result.Length.ShouldBe(4);
            buffer.ItemsAvailable.ShouldBe(3);
        }

        [Fact]
        public void WriteShouldThrowExceptionIfDataNull()
        {
            var buffer = new CircularBuffer<int>(10);

            Should.Throw<ArgumentNullException>(() => _ = buffer.Write(null));
        }

        [Fact]
        public void WriteShouldWriteAllDataIfSpaceAvailable()
        {
            var buffer = new CircularBuffer<int>(10);
            var data = new int[8];

            var result = buffer.Write(data);

            result.ShouldBe(data.Length);
        }

        [Fact]
        public void WriteShouldWritePartialDataIfSpaceNotAvailable()
        {
            var buffer = new CircularBuffer<int>(10);
            var data = new int[15];

            var result = buffer.Write(data);

            result.ShouldBe(buffer.Length);
        }

        [Fact]
        public void WriteShouldWriteAroundBoundry()
        {
            var buffer = new CircularBuffer<int>(10);

            _ = buffer.Write(new int[8]);
            _ = buffer.Read(4);
            var result = buffer.Write(new int[4]);

            result.ShouldBe(4);
            buffer.ItemsAvailable.ShouldBe(8);
        }

        [Fact]
        public void ItemsAvailableShouldUpdateWhenWrittenTo()
        {
            var buffer = new CircularBuffer<int>(10);
            var data = new int[8];

            _ = buffer.Write(data);

            buffer.ItemsAvailable.ShouldBe(data.Length);
        }

        [Fact]
        public void ItemsAvailableShouldUpdateWhenReadFrom()
        {
            var buffer = new CircularBuffer<int>(10);

            _ = buffer.Write(new int[8]);
            _ = buffer.Read(4);

            buffer.ItemsAvailable.ShouldBe(4);
        }

    }
}
