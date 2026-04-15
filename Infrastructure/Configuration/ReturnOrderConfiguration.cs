using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ReturnOrderConfiguration : IEntityTypeConfiguration<ReturnOrder>
    {
        public void Configure(EntityTypeBuilder<ReturnOrder> builder)
        {
            builder.ToTable("ReturnOrders");
            builder.HasKey(ro => ro.Id);
            builder.Property(ro => ro.Reason).IsRequired().HasMaxLength(500);
            builder.Property(ro => ro.RefundAmount).HasConversion(
                money => money.Amount,
                value => Domain.ValueObjects.Money.Vnd(value));
            builder.HasIndex(ro => ro.OriginalOrderId);
            builder.HasIndex(ro => ro.Status);
            builder.Ignore(ro => ro.DomainEvents);
        }
    }
}
