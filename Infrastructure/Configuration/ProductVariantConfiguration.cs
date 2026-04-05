using Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");
            builder.HasKey(v => v.Id);
            
            builder.Property(v => v.Sku).IsRequired().HasMaxLength(50);
            builder.Property(v => v.Price).HasPrecision(18, 2);
            builder.Property(v => v.AttributesJson).HasColumnType("nvarchar(max)");
            builder.Property(v => v.IsActive).HasDefaultValue(true);
            
            builder.HasIndex(v => v.Sku).IsUnique();
            builder.HasIndex(v => v.ProductId);
            
            builder.HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Ignore(v => v.DomainEvents);
        }
    }
}
