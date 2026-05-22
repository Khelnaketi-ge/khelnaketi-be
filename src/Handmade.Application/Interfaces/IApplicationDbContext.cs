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
    DbSet<Brand> Brands { get; }
    DbSet<BrandRole> BrandRoles { get; }
    DbSet<BrandMember> BrandMembers { get; }
    DbSet<BrandInvitation> BrandInvitations { get; }
    DbSet<BrandAddress> BrandAddresses { get; }
    DbSet<BrandEmailAddress> BrandEmailAddresses { get; }
    DbSet<BrandPhoneNumber> BrandPhoneNumbers { get; }
    DbSet<ImageAsset> ImageAssets { get; }
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
