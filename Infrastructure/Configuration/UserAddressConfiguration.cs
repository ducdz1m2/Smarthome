using Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
    {
        public void Configure(EntityTypeBuilder<UserAddress> builder)
        {
            builder.ToTable("UserAddresses");
            builder.HasKey(ua => ua.Id);
            builder.Property(ua => ua.Label).IsRequired().HasMaxLength(50);
            builder.Property(ua => ua.ReceiverName).IsRequired().HasMaxLength(100);
            builder.Property(ua => ua.IsDefault).HasDefaultValue(false);
            builder.HasIndex(ua => ua.UserId);
            builder.OwnsOne(ua => ua.Address, a => { a.Property(ad => ad.Street).HasMaxLength(200); a.Property(ad => ad.Ward).HasMaxLength(50); a.Property(ad => ad.District).HasMaxLength(50); a.Property(ad => ad.City).HasMaxLength(50); });
            builder.OwnsOne(ua => ua.ReceiverPhone, p => { p.Property(ph => ph.Value).HasMaxLength(20).HasColumnName("ReceiverPhone"); });
            builder.Ignore(ua => ua.DomainEvents);
        }
    }
}
