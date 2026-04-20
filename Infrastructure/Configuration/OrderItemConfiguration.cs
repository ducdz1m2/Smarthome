using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");
            builder.HasKey(oi => oi.Id);
            builder.Property(oi => oi.UnitPrice).HasConversion(
                money => money.Amount,
                value => Domain.ValueObjects.Money.Vnd(value));
            builder.HasIndex(oi => oi.OrderId);
            builder.HasIndex(oi => oi.ProductId);

            // Ignore navigation properties to prevent tracking conflicts
            builder.Ignore(oi => oi.InstallationBooking);

            builder.Ignore(oi => oi.DomainEvents);
        }
    }
}
