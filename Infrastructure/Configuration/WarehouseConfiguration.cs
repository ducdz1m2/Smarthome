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
            builder.Property(w => w.IsActive).HasDefaultValue(true);
            
            builder.HasIndex(w => w.Code).IsUnique();
            builder.HasIndex(w => w.IsActive);
            
            builder.OwnsOne(w => w.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(200);
                address.Property(a => a.Ward).HasMaxLength(50);
                address.Property(a => a.District).HasMaxLength(50);
                address.Property(a => a.City).HasMaxLength(50);
            });
            
            builder.OwnsOne(w => w.Phone, phone =>
            {
                phone.Property(p => p.Value).HasMaxLength(20);
                phone.Property(p => p.FormattedValue).HasMaxLength(20);
            });
            
            builder.Ignore(w => w.DomainEvents);
        }
    }
}
