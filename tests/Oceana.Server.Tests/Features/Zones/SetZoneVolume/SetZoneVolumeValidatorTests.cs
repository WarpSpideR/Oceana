namespace Oceana.Server.Features.Zones.SetZoneVolume;

public class SetZoneVolumeValidatorTests
{
    private readonly SetZoneVolumeValidator validator = new SetZoneVolumeValidator();

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Passes_ForVolumeInRange(double volume)
    {
        this.validator.Validate(new SetZoneVolumeRequest { Volume = volume }).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Fails_ForVolumeOutOfRange(double volume)
    {
        this.validator.Validate(new SetZoneVolumeRequest { Volume = volume }).IsValid.Should().BeFalse();
    }
}
