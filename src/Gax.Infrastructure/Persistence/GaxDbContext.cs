using Gax.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gax.Infrastructure.Persistence;

public class GaxDbContext : DbContext
{
    public GaxDbContext(DbContextOptions<GaxDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Right> Rights => Set<Right>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RoleRight> RoleRights => Set<RoleRight>();
    public DbSet<ResourceRight> ResourceRights => Set<ResourceRight>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GaxDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
