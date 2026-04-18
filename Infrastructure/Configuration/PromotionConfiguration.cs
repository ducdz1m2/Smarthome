using Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
    {
        public void Configure(EntityTypeBuilder<Promotion> builder)
        {
            builder.ToTable("Promotions");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
            builder.Property(p => p.Description).HasMaxLength(1000);
            
            builder.Property(p => p.DiscountPercent).HasConversion(
                percent => percent!.Value,
                value => Domain.ValueObjects.Percentage.Create(value!));
            
            builder.Property(p => p.MinOrderAmount).HasConversion(
                money => money!.Amount,
                value => Domain.ValueObjects.Money.Vnd(value!));
            builder.Property(p => p.IsActive).HasDefaultValue(true);
            builder.HasIndex(p => p.StartDate);
            builder.HasIndex(p => p.EndDate);
            builder.HasIndex(p => p.IsActive);
            builder.HasMany(p => p.PromotionProducts).WithOne(pp => pp.Promotion).HasForeignKey(pp => pp.PromotionId).OnDelete(DeleteBehavior.Cascade);
            builder.Ignore(p => p.DomainEvents);
        }
    }
}
