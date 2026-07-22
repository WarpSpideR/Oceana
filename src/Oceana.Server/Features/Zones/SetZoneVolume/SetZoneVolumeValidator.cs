using FastEndpoints;
using FluentValidation;

namespace Oceana.Server.Features.Zones.SetZoneVolume;

/// <summary>
/// Validates <see cref="SetZoneVolumeRequest"/>.
/// </summary>
public sealed class SetZoneVolumeValidator : Validator<SetZoneVolumeRequest>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SetZoneVolumeValidator"/> class.
    /// </summary>
    public SetZoneVolumeValidator()
    {
        RuleFor(x => x.Volume)
            .InclusiveBetween(0.0, 1.0)
            .WithMessage("Volume must be between 0 and 1.");
    }
}
