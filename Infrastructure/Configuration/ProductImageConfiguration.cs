using Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Url).IsRequired().HasMaxLength(500);
            builder.Property(i => i.AltText).HasMaxLength(200);
            builder.Property(i => i.IsMain).HasDefaultValue(false);
            builder.HasIndex(i => i.ProductId);
            builder.HasIndex(i => new { i.ProductId, i.IsMain });
            builder.HasOne(i => i.Product).WithMany(p => p.Images).HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
            builder.Ignore(i => i.DomainEvents);
        }
    }
}
