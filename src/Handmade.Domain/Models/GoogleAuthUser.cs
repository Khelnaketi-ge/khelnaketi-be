namespace Handmade.Domain.Models;

public sealed record GoogleAuthUser(
    string ProviderUserId,
    string Email,
    bool EmailVerified,
    string? FullName,
    string? FirstName,
    string? LastName,
    string? PictureUrl
);