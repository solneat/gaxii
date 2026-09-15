using Gax.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.MetadataBuilders;

namespace Gax.Infrastructure.Persistence.Configurations;

public class RoleRightConfiguration : IEntityTypeConfiguration<RoleRight>
{
    public void Configure(EntityTypeBuilder<RoleRight> b)
    {
        b.ToTable("RoleRights");
        b.HasKey(x => new { x.RoleId, x.RightId });

        b.HasOne(x => x.Role)
            .WithMany(x => x.RoleRights)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Right)
            .WithMany(x => x.RoleRights)
            .HasForeignKey(x => x.RightId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
