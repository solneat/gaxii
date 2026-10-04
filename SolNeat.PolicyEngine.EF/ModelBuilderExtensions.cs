using Microsoft.EntityFrameworkCore;

namespace SolNeat.PolicyEngine.EF;

public static class ModelBuilderExtensions
{
    public static void UseSolNeatPolicyEngine(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Actor>(entity =>
        {
            entity.ToTable("solneat_actors");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PreferredName).HasMaxLength(100);
            entity.HasIndex(e => e.SubjectId).IsUnique();
        });

        modelBuilder.Entity<Solution>(entity =>
        {
            entity.ToTable("solneat_solutions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("solneat_roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.HasIndex(e => new { e.SolutionId, e.Name }).IsUnique();
            entity.HasOne(r => r.Solution)
                .WithMany(s => s.Roles)
                .HasForeignKey(r => r.SolutionId)
                .OnDelete(DeleteBehavior.Cascade);
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

        // ==========================================
        // Authentication Factors Configuration (v2.0.0)
        // ==========================================

        modelBuilder.Entity<ActorTotpSecret>(entity =>
        {
            entity.ToTable("solneat_actor_totp_secrets");
            entity.HasKey(t => t.ActorId);

            entity.Property(t => t.Secret)
                  .IsRequired()
                  .HasMaxLength(128);

            entity.HasOne<Actor>()
                  .WithOne()
                  .HasForeignKey<ActorTotpSecret>(t => t.ActorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActorPasskey>(entity =>
        {
            entity.ToTable("solneat_actor_passkeys");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.CredentialId).IsRequired();
            entity.Property(p => p.PublicKey).IsRequired();
            entity.Property(p => p.UserHandle).IsRequired();
            entity.Property(p => p.DeviceName).HasMaxLength(128);

            // Fast lookup when receiving credentialId from the WebAuthn API
            entity.HasIndex(p => p.CredentialId).IsUnique();

            // Fast lookup for listing user passkeys
            entity.HasIndex(p => p.ActorId);

            entity.HasOne<Actor>()
                  .WithMany()
                  .HasForeignKey(p => p.ActorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
