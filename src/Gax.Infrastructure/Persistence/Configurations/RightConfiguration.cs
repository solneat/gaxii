using Gax.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.MetadataBuilders;

namespace Gax.Infrastructure.Persistence.Configurations;

public class RightConfiguration : IEntityTypeConfiguration<Right>
{
    public void Configure(EntityTypeBuilder<Right> b)
    {
        b.ToTable("Rights");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(128).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();

        b.HasMany(x => x.RoleRights)
            .WithOne(x => x.Right)
            .HasForeignKey(x => x.RightId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.ResourceRights)
            .WithOne(x => x.Right)
            .HasForeignKey(x => x.RightId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
