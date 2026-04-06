using Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.ToTable("Coupons");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
            builder.Property(c => c.DiscountValue).HasPrecision(18, 2);
            builder.Property(c => c.MinOrderAmount).HasPrecision(18, 2);
            builder.Property(c => c.MaxDiscountAmount).HasPrecision(18, 2);
            builder.Property(c => c.IsActive).HasDefaultValue(true);
            builder.HasIndex(c => c.Code).IsUnique();
            builder.HasIndex(c => c.ExpiryDate);
            builder.HasIndex(c => c.IsActive);
            builder.Ignore(c => c.DomainEvents);
        }
    }
}
