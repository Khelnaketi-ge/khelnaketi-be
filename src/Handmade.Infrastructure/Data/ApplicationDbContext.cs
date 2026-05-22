using System.Reflection;
using Handmade.Application.Interfaces;
using Handmade.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Handmade.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<BrandRole> BrandRoles => Set<BrandRole>();
    public DbSet<BrandMember> BrandMembers => Set<BrandMember>();
    public DbSet<BrandInvitation> BrandInvitations => Set<BrandInvitation>();
    public DbSet<BrandAddress> BrandAddresses => Set<BrandAddress>();
    public DbSet<BrandEmailAddress> BrandEmailAddresses => Set<BrandEmailAddress>();
    public DbSet<BrandPhoneNumber> BrandPhoneNumbers => Set<BrandPhoneNumber>();
    public DbSet<ImageAsset> ImageAssets => Set<ImageAsset>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await Database.BeginTransactionAsync(cancellationToken);
    }
}
