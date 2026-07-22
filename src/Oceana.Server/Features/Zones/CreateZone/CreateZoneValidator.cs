using FastEndpoints;
using FluentValidation;

namespace Oceana.Server.Features.Zones.CreateZone;

/// <summary>
/// Validates <see cref="CreateZoneRequest"/>.
/// </summary>
public sealed class CreateZoneValidator : Validator<CreateZoneRequest>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="CreateZoneValidator"/> class.
    /// </summary>
    public CreateZoneValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("A zone must have a name.");

        RuleForEach(x => x.Devices).ChildRules(device =>
        {
            device.RuleFor(d => d.AgentId).NotEqual(Guid.Empty).WithMessage("Each device must reference an agent.");
            device.RuleFor(d => d.DeviceId).NotEmpty().WithMessage("Each device must reference a device.");
        });

        RuleFor(x => x.Devices)
            .Must(devices => devices.Select(d => (d.AgentId, d.DeviceId)).Distinct().Count() == devices.Count)
            .WithMessage("A device cannot be listed more than once in a zone.");
    }
}
