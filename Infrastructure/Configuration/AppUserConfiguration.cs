using Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);
            builder.Property(u => u.UserName).IsRequired().HasMaxLength(50);
            builder.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            builder.Property(u => u.Avatar).HasMaxLength(500);
            builder.Property(u => u.IsActive).HasDefaultValue(true);
            builder.HasIndex(u => u.UserName).IsUnique();
            builder.HasIndex(u => u.Email).IsUnique();
            builder.OwnsOne(u => u.Email, e => e.Property(em => em.Value).HasMaxLength(100).HasColumnName("Email"));
            builder.OwnsOne(u => u.PhoneNumber, p => p.Property(ph => ph.Value).HasMaxLength(20).HasColumnName("PhoneNumber"));
            builder.Ignore(u => u.DomainEvents);
        }
    }
}
