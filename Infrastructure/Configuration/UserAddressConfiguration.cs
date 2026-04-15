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
            builder.Property(ua => ua.ReceiverPhone).HasConversion(
                phone => phone.ToString(),
                value => Domain.ValueObjects.PhoneNumber.Create(value));
            builder.OwnsOne(ua => ua.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200);
                address.Property(a => a.Ward).HasColumnName("Ward").HasMaxLength(50);
                address.Property(a => a.District).HasColumnName("District").HasMaxLength(50);
                address.Property(a => a.City).HasColumnName("City").HasMaxLength(50);
                address.Property(a => a.Country).HasColumnName("Country").HasMaxLength(50);
                address.Property(a => a.PostalCode).HasColumnName("PostalCode").HasMaxLength(10);
            });
            builder.Property(ua => ua.IsDefault).HasDefaultValue(false);
            builder.HasIndex(ua => ua.UserId);
            builder.Ignore(ua => ua.DomainEvents);
        }
    }
}
