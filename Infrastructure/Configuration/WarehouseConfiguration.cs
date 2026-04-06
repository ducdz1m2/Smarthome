using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("Warehouses");
            builder.HasKey(w => w.Id);
            
            builder.Property(w => w.Name).IsRequired().HasMaxLength(100);
            builder.Property(w => w.Code).IsRequired().HasMaxLength(20);
            builder.Property(w => w.ManagerName).HasMaxLength(100);
            builder.Property(w => w.AddressStreet).IsRequired().HasMaxLength(200);
            builder.Property(w => w.AddressWard).HasMaxLength(50);
            builder.Property(w => w.AddressDistrict).HasMaxLength(50);
            builder.Property(w => w.AddressCity).HasMaxLength(50);
            builder.Property(w => w.Phone).HasMaxLength(20);
            builder.Property(w => w.IsActive).HasDefaultValue(true);
            
            builder.HasIndex(w => w.Code).IsUnique();
            builder.HasIndex(w => w.IsActive);
            
            builder.Ignore(w => w.DomainEvents);
        }
    }
}
