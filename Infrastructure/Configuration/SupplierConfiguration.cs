using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ToTable("Suppliers");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
            builder.Property(s => s.TaxCode).HasMaxLength(50);
            builder.Property(s => s.ContactName).HasMaxLength(100);
            builder.Property(s => s.BankAccount).HasMaxLength(50);
            builder.Property(s => s.BankName).HasMaxLength(100);
            builder.OwnsOne(s => s.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
                address.Property(a => a.Ward).HasColumnName("AddressWard").HasMaxLength(50);
                address.Property(a => a.District).HasColumnName("AddressDistrict").HasMaxLength(50);
                address.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(50);
                address.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(50);
                address.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(10);
            });
            builder.Property(s => s.Phone).HasConversion(
                phone => phone.ToString(),
                value => Domain.ValueObjects.PhoneNumber.Create(value));
            builder.Property(s => s.Email).HasConversion(
                email => email.ToString(),
                value => Domain.ValueObjects.Email.Create(value));
            builder.Property(s => s.IsActive).HasDefaultValue(true);
            builder.HasIndex(s => s.Name);
            builder.HasIndex(s => s.IsActive);
            builder.Ignore(s => s.DomainEvents);
        }
    }
}
