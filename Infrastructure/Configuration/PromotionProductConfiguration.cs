using Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class PromotionProductConfiguration : IEntityTypeConfiguration<PromotionProduct>
    {
        public void Configure(EntityTypeBuilder<PromotionProduct> builder)
        {
            builder.ToTable("PromotionProducts");
            builder.HasKey(pp => pp.Id);
            builder.Property(pp => pp.CustomDiscountPercent).HasConversion(
                percent => percent != null ? percent.Value : (decimal?)null,
                value => value.HasValue ? Domain.ValueObjects.Percentage.Create(value.Value) : null);
            builder.HasIndex(pp => new { pp.PromotionId, pp.ProductId }).IsUnique();
            builder.HasOne(pp => pp.Promotion).WithMany(p => p.PromotionProducts).HasForeignKey(pp => pp.PromotionId).OnDelete(DeleteBehavior.Cascade);
            builder.Ignore(pp => pp.DomainEvents);
        }
    }
}
