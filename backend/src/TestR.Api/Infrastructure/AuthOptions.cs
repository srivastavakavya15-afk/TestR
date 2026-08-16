using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TestR.Api.Infrastructure;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public bool Enabled { get; init; }

    public string? Authority { get; init; }

    public string? Audience { get; init; }

    public bool RequireHttpsMetadata { get; init; } = true;

    [MemberNotNullWhen(true, nameof(Authority))]
    public bool IsConfigured => Enabled && !string.IsNullOrWhiteSpace(Authority);

    public void ValidateOnStart()
    {
        if (Enabled && string.IsNullOrWhiteSpace(Authority))
        {
            throw new ValidationException(
                $"{SectionName}:Authority is required when {SectionName}:Enabled is true.");
        }
    }
}
