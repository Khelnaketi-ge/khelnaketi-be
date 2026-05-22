using Handmade.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Handmade.Application.Interfaces;

public interface IApplicationDbContext : IDisposable
{
    DbSet<User> Users { get; }
    DbSet<UserExternalLogin> UserExternalLogins { get; }
    DbSet<UserSession> UserSessions { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<VerificationCode> VerificationCodes { get; }
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
