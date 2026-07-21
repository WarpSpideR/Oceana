using FastEndpoints;
using FluentValidation;

namespace Oceana.Server.Features.Agents.RegisterAgent;

/// <summary>
/// Validates <see cref="RegisterAgentRequest"/>.
/// </summary>
public sealed class RegisterAgentValidator : Validator<RegisterAgentRequest>
{
    /// <summary>
    /// Initialises a new instance of the <see cref="RegisterAgentValidator"/> class.
    /// </summary>
    public RegisterAgentValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Host).NotEmpty();
        RuleFor(x => x.Port).InclusiveBetween(1, 65535);
    }
}
