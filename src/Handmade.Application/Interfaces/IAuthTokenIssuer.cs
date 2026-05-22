using Handmade.Application.Common.Models.Auth;
using Handmade.Domain.Entities;

namespace Handmade.Application.Interfaces;

public interface IAuthTokenIssuer
{
    Task<TokensModel> IssueTokensAsync(User user, CancellationToken cancellationToken);
}
