using NAudio.Wave;
using Oceana.Agent.Windows.Protocol;

namespace Oceana.Agent.Windows.Test.Protocol;

public class AudioStreamHeaderTests
{
    [Fact]
    public void WriteThenParse_PreservesAllFields()
    {
        var header = new AudioStreamHeader(AudioEncoding.Pcm, channels: 2, sampleRate: 48000, bitsPerSample: 16);
        var buffer = new byte[AudioStreamHeader.Size];

        header.Write(buffer);
        var parsed = AudioStreamHeader.Parse(buffer);

        parsed.Encoding.Should().Be(AudioEncoding.Pcm);
        parsed.Channels.Should().Be(2);
        parsed.SampleRate.Should().Be(48000);
        parsed.BitsPerSample.Should().Be(16);
    }

    [Fact]
    public void Parse_WithWrongMagic_ThrowsInvalidData()
    {
        var buffer = new byte[AudioStreamHeader.Size];

        var act = () => AudioStreamHeader.Parse(buffer);

        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public void Parse_WithUnsupportedVersion_ThrowsInvalidData()
    {
        var header = new AudioStreamHeader(AudioEncoding.Pcm, 2, 48000, 16);
        var buffer = new byte[AudioStreamHeader.Size];
        header.Write(buffer);
        buffer[4] = 0x7F;

        var act = () => AudioStreamHeader.Parse(buffer);

        act.Should().Throw<InvalidDataException>();
    }

    [Fact]
    public async Task ReadAsync_ReassemblesHeaderDeliveredOneByteAtATime()
    {
        var header = new AudioStreamHeader(AudioEncoding.IeeeFloat, channels: 2, sampleRate: 44100, bitsPerSample: 32);
        var buffer = new byte[AudioStreamHeader.Size];
        header.Write(buffer);
        await using var stream = new DripStream(buffer);

        var parsed = await AudioStreamHeader.ReadAsync(stream, CancellationToken.None);

        parsed.Encoding.Should().Be(AudioEncoding.IeeeFloat);
        parsed.Channels.Should().Be(2);
        parsed.SampleRate.Should().Be(44100);
        parsed.BitsPerSample.Should().Be(32);
    }

    [Fact]
    public void ToWaveFormat_Pcm_MapsToPcmWaveFormat()
    {
        var header = new AudioStreamHeader(AudioEncoding.Pcm, 2, 48000, 16);

        var format = header.ToWaveFormat();

        format.Encoding.Should().Be(WaveFormatEncoding.Pcm);
        format.SampleRate.Should().Be(48000);
        format.Channels.Should().Be(2);
        format.BitsPerSample.Should().Be(16);
    }

    [Fact]
    public void ToWaveFormat_IeeeFloat_MapsToFloatWaveFormat()
    {
        var header = new AudioStreamHeader(AudioEncoding.IeeeFloat, 2, 48000, 32);

        var format = header.ToWaveFormat();

        format.Encoding.Should().Be(WaveFormatEncoding.IeeeFloat);
        format.SampleRate.Should().Be(48000);
        format.Channels.Should().Be(2);
    }

    /// <summary>
    /// A read-only stream that yields at most one byte per read, to exercise fragmented-transport reassembly.
    /// </summary>
    private sealed class DripStream(byte[] data) : Stream
    {
        private int position;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => data.Length;

        public override long Position
        {
            get => position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0 || position >= data.Length)
            {
                return 0;
            }

            buffer[offset] = data[position];
            position++;
            return 1;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
