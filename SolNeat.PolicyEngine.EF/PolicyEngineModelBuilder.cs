using Microsoft.EntityFrameworkCore;

namespace SolNeat.PolicyEngine.EF;

public static class PolicyEngineModelBuilder
{
    public static void UseSolNeatPolicyEngine(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Actor>(entity =>
        {
            entity.ToTable("solneat_actors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.HasIndex(e => new { e.Solution, e.SubjectId }).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("solneat_roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Solution).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.HasIndex(e => new { e.Solution, e.Name }).IsUnique();
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("solneat_permissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Resource, e.Action }).IsUnique();
        });

        modelBuilder.Entity<ActorRole>(entity =>
        {
            entity.ToTable("solneat_actor_roles");
            entity.HasKey(ar => new { ar.ActorId, ar.RoleId });

            entity.HasOne(ar => ar.Actor)
                  .WithMany(a => a.ActorRoles)
                  .HasForeignKey(ar => ar.ActorId)
                  .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ActorRole>()
                  .HasOne(ar => ar.Role)
                  .WithMany(r => r.ActorRoles)
                  .HasForeignKey(ar => ar.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("solneat_role_permissions");
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            entity.HasOne(rp => rp.Role)
                  .WithMany(r => r.RolePermissions)
                  .HasForeignKey(rp => rp.RoleId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                  .WithMany(p => p.RolePermissions)
                  .HasForeignKey(rp => rp.PermissionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
