using Handmade.Application.Common.Models.Auth;
using Handmade.Domain.Enums;

namespace Handmade.Application.Interfaces;

public interface IAuthService
{
    Task RegisterAsync(
        string firstName, string lastName, 
        string email, string password, 
        string confirmPassword, CancellationToken cancellationToken);
    Task<TokensModel> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<TokensModel> PanelLoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<TokensModel> RefreshAsync(string accessToken, string refreshToken, CancellationToken cancellationToken);
    Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken);
    Task ResetPasswordAsync(
        string email,
        string code,
        string newPassword,
        string confirmNewPassword,
        CancellationToken cancellationToken);
    Task ResendVerificationCodeAsync(string email, VerificationCodePurpose purpose, CancellationToken cancellationToken);
    Task ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken cancellationToken);
    Task SendEmailCodeAsync(int userId, CancellationToken cancellationToken);
    Task<TokensModel> VerifyEmailCodeAsync(string email, string code, CancellationToken cancellationToken);
}
