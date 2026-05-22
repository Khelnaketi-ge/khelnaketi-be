using Handmade.Domain.Enums;

namespace Handmade.Application.Common.Models.Auth;

public sealed record ExternalLoginModel(
    Provider Provider,
    string ProviderUserId,
    string? Email,
    bool EmailVerified,
    string? DisplayName);
