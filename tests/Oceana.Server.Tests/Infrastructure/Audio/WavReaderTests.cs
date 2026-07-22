using System.IO;

namespace Oceana.Server.Infrastructure.Audio;

public class WavReaderTests
{
    [Fact]
    public void TryRead_ParsesMono48k16BitPcm()
    {
        var pcm = new byte[] { 1, 0, 2, 0, 3, 0, 4, 0 };
        var wav = BuildWav(audioFormat: 1, channels: 1, sampleRate: 48000, bits: 16, data: pcm);

        var ok = WavReader.TryRead(wav, out var audio, out var error);

        ok.Should().BeTrue();
        error.Should().BeNull();
        audio!.Format.Channels.Should().Be(1);
        audio.Format.SampleRate.Should().Be(48000);
        audio.Format.BitsPerSample.Should().Be(16);
        audio.Pcm.Should().Equal(pcm);
    }

    [Fact]
    public void TryRead_AcceptsStereo()
    {
        var pcm = new byte[] { 1, 0, 2, 0, 3, 0, 4, 0 };
        var wav = BuildWav(1, channels: 2, sampleRate: 48000, bits: 16, data: pcm);

        var ok = WavReader.TryRead(wav, out var audio, out _);

        ok.Should().BeTrue();
        audio!.Format.Channels.Should().Be(2);
        audio.Pcm.Should().Equal(pcm);
    }

    [Fact]
    public void TryRead_RejectsMoreThanStereo()
    {
        var wav = BuildWav(1, channels: 6, sampleRate: 48000, bits: 16, data: new byte[] { 0, 0 });

        WavReader.TryRead(wav, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void TryRead_RejectsWrongSampleRate()
    {
        var wav = BuildWav(1, channels: 1, sampleRate: 44100, bits: 16, data: new byte[] { 0, 0 });

        WavReader.TryRead(wav, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void TryRead_RejectsEightBit()
    {
        var wav = BuildWav(1, channels: 1, sampleRate: 48000, bits: 8, data: new byte[] { 0, 0 });

        WavReader.TryRead(wav, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void TryRead_RejectsNonPcmEncoding()
    {
        // 3 = IEEE float
        var wav = BuildWav(audioFormat: 3, channels: 1, sampleRate: 48000, bits: 16, data: new byte[] { 0, 0 });

        WavReader.TryRead(wav, out _, out _).Should().BeFalse();
    }

    [Fact]
    public void TryRead_RejectsNonWavData()
    {
        WavReader.TryRead(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, out _, out var error).Should().BeFalse();
        error.Should().NotBeNull();
    }

    [Fact]
    public void TryRead_RejectsEmptyDataChunk()
    {
        var wav = BuildWav(1, 1, 48000, 16, Array.Empty<byte>());

        WavReader.TryRead(wav, out _, out _).Should().BeFalse();
    }

    private static byte[] BuildWav(ushort audioFormat, ushort channels, int sampleRate, ushort bits, byte[] data)
    {
        var blockAlign = (ushort)(channels * bits / 8);
        var byteRate = sampleRate * blockAlign;

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + data.Length);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write(audioFormat);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(bits);
        writer.Write("data"u8.ToArray());
        writer.Write(data.Length);
        writer.Write(data);
        writer.Flush();
        return ms.ToArray();
    }
}
