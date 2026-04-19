using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);
            
            builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
            builder.Property(o => o.ReceiverName).IsRequired().HasMaxLength(100);
            builder.Property(o => o.ReceiverPhone).HasConversion(
                phone => phone.ToString(),
                value => Domain.ValueObjects.PhoneNumber.Create(value));
            builder.OwnsOne(o => o.ShippingAddress, address =>
            {
                address.Property(a => a.Street).HasColumnName("ShippingAddressStreet").HasMaxLength(200);
                address.Property(a => a.Ward).HasColumnName("ShippingAddressWard").HasMaxLength(50);
                address.Property(a => a.District).HasColumnName("ShippingAddressDistrict").HasMaxLength(50);
                address.Property(a => a.City).HasColumnName("ShippingAddressCity").HasMaxLength(50);
                address.Property(a => a.Country).HasColumnName("ShippingAddressCountry").HasMaxLength(50);
                address.Property(a => a.PostalCode).HasColumnName("ShippingAddressPostalCode").HasMaxLength(10);
            });
            builder.Property(o => o.StatusHistoryJson).HasColumnType("nvarchar(max)");
            builder.Property(o => o.CancelReason).HasMaxLength(500);
            
            builder.Property(o => o.TotalAmount).HasConversion(
                money => money.Amount,
                value => Domain.ValueObjects.Money.Vnd(value));
            builder.Property(o => o.ShippingFee).HasConversion(
                money => money.Amount,
                value => Domain.ValueObjects.Money.Vnd(value));
            builder.Property(o => o.DiscountAmount).HasConversion(
                money => money.Amount,
                value => Domain.ValueObjects.Money.Vnd(value));
            
            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.HasIndex(o => o.UserId);
            builder.HasIndex(o => o.Status);
            builder.HasIndex(o => o.CreatedAt);
            
            builder.HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(o => o.Shipments)
                .WithOne(os => os.Order)
                .HasForeignKey(os => os.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.PaymentTransaction)
                .WithOne()
                .HasForeignKey<PaymentTransaction>(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(o => o.DomainEvents);
        }
    }
}
