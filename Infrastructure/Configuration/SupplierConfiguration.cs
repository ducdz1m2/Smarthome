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
            builder.Property(s => s.IsActive).HasDefaultValue(true);
            builder.HasIndex(s => s.Name);
            builder.HasIndex(s => s.IsActive);
            builder.OwnsOne(s => s.Address, a => { a.Property(ad => ad.Street).HasMaxLength(200); a.Property(ad => ad.Ward).HasMaxLength(50); a.Property(ad => ad.District).HasMaxLength(50); a.Property(ad => ad.City).HasMaxLength(50); });
            builder.OwnsOne(s => s.Phone, p => { p.Property(ph => ph.Value).HasMaxLength(20); });
            builder.OwnsOne(s => s.Email, e => { e.Property(em => em.Value).HasMaxLength(100); });
            builder.Ignore(s => s.DomainEvents);
        }
    }
}
