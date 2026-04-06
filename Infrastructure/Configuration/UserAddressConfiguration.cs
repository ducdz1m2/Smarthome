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
            builder.Property(ua => ua.ReceiverPhone).IsRequired().HasMaxLength(20);
            builder.Property(ua => ua.Street).IsRequired().HasMaxLength(200);
            builder.Property(ua => ua.Ward).HasMaxLength(50);
            builder.Property(ua => ua.District).HasMaxLength(50);
            builder.Property(ua => ua.City).HasMaxLength(50);
            builder.Property(ua => ua.IsDefault).HasDefaultValue(false);
            builder.HasIndex(ua => ua.UserId);
            builder.Ignore(ua => ua.DomainEvents);
        }
    }
}
