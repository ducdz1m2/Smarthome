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
            builder.Property(o => o.ReceiverPhone).IsRequired().HasMaxLength(20);
            builder.Property(o => o.ShippingAddressStreet).IsRequired().HasMaxLength(200);
            builder.Property(o => o.ShippingAddressWard).HasMaxLength(50);
            builder.Property(o => o.ShippingAddressDistrict).HasMaxLength(50);
            builder.Property(o => o.ShippingAddressCity).HasMaxLength(50);
            builder.Property(o => o.StatusHistoryJson).HasColumnType("nvarchar(max)");
            builder.Property(o => o.CancelReason).HasMaxLength(500);
            builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
            builder.Property(o => o.ShippingFee).HasPrecision(18, 2);
            builder.Property(o => o.DiscountAmount).HasPrecision(18, 2);
            
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
                
            builder.Ignore(o => o.DomainEvents);
        }
    }
}
