using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ProductRatingConfiguration : IEntityTypeConfiguration<ProductRating>
    {
        public void Configure(EntityTypeBuilder<ProductRating> builder)
        {
            builder.ToTable("ProductRatings");
            builder.HasKey(pr => pr.Id);
            
            builder.Property(pr => pr.Comment).HasMaxLength(1000);
            builder.Property(pr => pr.OrderItemId).IsRequired();
            
            builder.HasIndex(pr => new { pr.ProductId, pr.VariantId, pr.OrderItemId, pr.CustomerId }).IsUnique();
            builder.HasIndex(pr => pr.ProductId);
            builder.HasIndex(pr => pr.OrderItemId);
            builder.HasIndex(pr => pr.CustomerId);
            builder.HasIndex(pr => pr.Status);
            
            builder.Ignore(pr => pr.DomainEvents);
        }
    }
}
