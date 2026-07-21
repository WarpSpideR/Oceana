using FastEndpoints;
using FluentValidation;

namespace Oceana.Server.Features.Agents.SetRouting;

/// <summary>
/// Validates <see cref="SetRoutingRequest"/>.
/// </summary>
public sealed class SetRoutingValidator : Validator<SetRoutingRequest>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="SetRoutingValidator"/> class.
    /// </summary>
    public SetRoutingValidator()
    {
        RuleFor(x => x.Outputs)
            .Must(outputs => outputs.All(output => output.Channels.Length > 0 && output.Channels.All(channel => channel >= 0)))
            .WithMessage("Each output must route at least one non-negative channel index.");
    }
}
