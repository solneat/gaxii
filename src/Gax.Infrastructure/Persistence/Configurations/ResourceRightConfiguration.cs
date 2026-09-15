using Gax.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.MetadataBuilders;

namespace Gax.Infrastructure.Persistence.Configurations;

public class ResourceRightConfiguration : IEntityTypeConfiguration<ResourceRight>
{
    public void Configure(EntityTypeBuilder<ResourceRight> b)
    {
        b.ToTable("ResourceRights");
        b.HasKey(x => new { x.ResourceId, x.RightId });

        b.HasOne(x => x.Resource)
            .WithMany(x => x.ResourceRights)
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Right)
            .WithMany(x => x.ResourceRights)
            .HasForeignKey(x => x.RightId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
