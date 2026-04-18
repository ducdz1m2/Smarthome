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
            builder.OwnsOne(w => w.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
                address.Property(a => a.Ward).HasColumnName("AddressWard").HasMaxLength(50);
                address.Property(a => a.District).HasColumnName("AddressDistrict").HasMaxLength(50);
                address.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(50);
                address.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(50);
                address.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(10);
            });
            builder.Property(w => w.Phone).HasConversion(
                phone => phone!.ToString(),
                value => Domain.ValueObjects.PhoneNumber.Create(value!));
            builder.Property(w => w.IsActive).HasDefaultValue(true);
            
            builder.HasIndex(w => w.Code).IsUnique();
            builder.HasIndex(w => w.IsActive);
            
            builder.Ignore(w => w.DomainEvents);
        }
    }
}
