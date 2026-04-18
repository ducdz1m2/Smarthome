using Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Description).HasMaxLength(4000);
            builder.Property(p => p.SpecsJson).HasColumnType("nvarchar(max)");
            
            // Value Object Converters
            builder.Property(p => p.Sku).HasConversion(
                sku => sku.Value,
                value => Domain.ValueObjects.Sku.Create(value));

            builder.Property(p => p.IsActive).HasDefaultValue(true);
            builder.Property(p => p.RequiresInstallation).HasDefaultValue(false);
            
            builder.HasIndex(p => p.Sku).IsUnique();
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.BrandId);
            builder.HasIndex(p => p.IsActive);
            
            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.Ignore(p => p.DomainEvents);
        }
    }
}
