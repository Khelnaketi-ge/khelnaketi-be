namespace Handmade.Application.Common.Exceptions;

public sealed record Error(string Code, string Text);

public static class UnauthorizedErrors
{
    public static readonly Error InvalidCreds = new(nameof(InvalidCreds), "Invalid email or password");
    public static readonly Error UserBlocked = new(nameof(UserBlocked), "Your account is blocked");
    public static readonly Error UserLockedOut = new(
        nameof(UserLockedOut), "Your account is temporarily locked due to too many failed login attempts");
    public static readonly Error EmailNotVerified = new(nameof(EmailNotVerified), "Email address is not verified");
    public static readonly Error InvalidExternalLogin = new(nameof(InvalidExternalLogin), "Invalid external login");
    public static readonly Error InvalidRefreshToken = new(nameof(InvalidRefreshToken), "Invalid refresh token");
    public static readonly Error InvalidEmailVerificationCode = new(
        nameof(InvalidEmailVerificationCode), "Invalid email verification code");
    public static readonly Error EmailAlreadyVerified = new(
        nameof(EmailAlreadyVerified), "Email address is already verified");
    public static readonly Error TooManyVerificationCodeRequests = new(
        nameof(TooManyVerificationCodeRequests), "Please wait before requesting another code");
    public static readonly Error InvalidPasswordResetCode = new(
        nameof(InvalidPasswordResetCode), "Invalid password reset code");
}

public static class ImageErrors
{
    public static readonly Error ContentRequired = new(nameof(ContentRequired), "Image content is required");
    public static readonly Error InvalidSize = new(nameof(InvalidSize), "Image size is invalid");
    public static readonly Error ContentTypeRequired = new(nameof(ContentTypeRequired), "Image content type is required");
    public static readonly Error ContentTypeNotAllowed = new(nameof(ContentTypeNotAllowed), "Image content type is not allowed");
    public static readonly Error InvalidFolder = new(nameof(InvalidFolder), "Image folder is invalid");
    public static readonly Error InvalidObjectKey = new(nameof(InvalidObjectKey), "Image object key is invalid");
}
