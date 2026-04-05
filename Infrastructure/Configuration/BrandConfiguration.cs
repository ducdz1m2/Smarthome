using Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
            builder.Property(b => b.Description).HasMaxLength(500);
            builder.Property(b => b.LogoUrl).HasMaxLength(500);
            builder.Property(b => b.IsActive).HasDefaultValue(true);
            builder.OwnsOne(b => b.Website, w => w.Property(wu => wu.Value).HasMaxLength(200));
            builder.HasIndex(b => b.Name).IsUnique();
            builder.HasIndex(b => b.IsActive);
            builder.Ignore(b => b.DomainEvents);
        }
    }
}
