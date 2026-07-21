using FastEndpoints;
using FluentValidation;

namespace Oceana.Server.Features.Agents.StartStream;

/// <summary>
/// Validates <see cref="StartStreamRequest"/>.
/// </summary>
public sealed class StartStreamValidator : Validator<StartStreamRequest>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="StartStreamValidator"/> class.
    /// </summary>
    public StartStreamValidator()
    {
        RuleFor(x => x.Frequency).InclusiveBetween(20d, 20000d);
        RuleFor(x => x.Channels).InclusiveBetween(1, 8);
        RuleFor(x => x.DurationSeconds!.Value)
            .InclusiveBetween(0.1, 3600d)
            .When(x => x.DurationSeconds.HasValue);
    }
}
