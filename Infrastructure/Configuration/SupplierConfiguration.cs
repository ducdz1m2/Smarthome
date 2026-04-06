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
            builder.Property(s => s.AddressStreet).HasMaxLength(200);
            builder.Property(s => s.AddressWard).HasMaxLength(50);
            builder.Property(s => s.AddressDistrict).HasMaxLength(50);
            builder.Property(s => s.AddressCity).HasMaxLength(50);
            builder.Property(s => s.Phone).HasMaxLength(20);
            builder.Property(s => s.Email).HasMaxLength(100);
            builder.Property(s => s.IsActive).HasDefaultValue(true);
            builder.HasIndex(s => s.Name);
            builder.HasIndex(s => s.IsActive);
            builder.Ignore(s => s.DomainEvents);
        }
    }
}
