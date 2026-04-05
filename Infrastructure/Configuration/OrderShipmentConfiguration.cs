using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class OrderShipmentConfiguration : IEntityTypeConfiguration<OrderShipment>
    {
        public void Configure(EntityTypeBuilder<OrderShipment> builder)
        {
            builder.ToTable("OrderShipments");
            builder.HasKey(os => os.Id);
            builder.Property(os => os.Carrier).IsRequired().HasMaxLength(50);
            builder.Property(os => os.TrackingNumber).IsRequired().HasMaxLength(100);
            builder.Property(os => os.Notes).HasMaxLength(500);
            builder.HasIndex(os => os.OrderId);
            builder.HasIndex(os => os.TrackingNumber).IsUnique();
            builder.HasOne(os => os.Order).WithMany(o => o.Shipments).HasForeignKey(os => os.OrderId).OnDelete(DeleteBehavior.Cascade);
            builder.Ignore(os => os.DomainEvents);
        }
    }
}
