using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gax.Identity;

public class AppIdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b => b.ToTable("IdentityUsers"));
        builder.Entity<ApplicationRole>(b => b.ToTable("IdentityRoles"));
        builder.Entity<IdentityUserRole<int>>(b => b.ToTable("IdentityUserRoles"));
        builder.Entity<IdentityUserClaim<int>>(b => b.ToTable("IdentityUserClaims"));
        builder.Entity<IdentityUserLogin<int>>(b => b.ToTable("IdentityUserLogins"));
        builder.Entity<IdentityUserToken<int>>(b => b.ToTable("IdentityUserTokens"));
        builder.Entity<IdentityRoleClaim<int>>(b => b.ToTable("IdentityRoleClaims"));
    }
}
