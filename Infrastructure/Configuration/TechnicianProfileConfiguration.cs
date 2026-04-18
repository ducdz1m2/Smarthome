using Domain.Entities.Installation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class TechnicianProfileConfiguration : IEntityTypeConfiguration<TechnicianProfile>
    {
        public void Configure(EntityTypeBuilder<TechnicianProfile> builder)
        {
            builder.ToTable("TechnicianProfiles");
            builder.HasKey(tp => tp.Id);
            builder.Property(tp => tp.Districts).IsRequired();
            builder.Property(tp => tp.SkillsJson).HasColumnType("nvarchar(max)");
            builder.Property(tp => tp.IsAvailable).HasDefaultValue(true);
            builder.Property(tp => tp.BaseSalary).HasConversion(
                money => money.Amount,
                value => Domain.ValueObjects.Money.Vnd(value));
            builder.Property(tp => tp.EmployeeCode).IsRequired();
            builder.Property(tp => tp.PhoneNumber).HasConversion(
                phone => phone!.ToString(),
                value => Domain.ValueObjects.PhoneNumber.Create(value!));
            builder.Property(tp => tp.Email).HasConversion(
                email => email!.ToString(),
                value => Domain.ValueObjects.Email.Create(value!));
            builder.OwnsOne(tp => tp.Address, address =>
            {
                address.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
                address.Property(a => a.Ward).HasColumnName("AddressWard").HasMaxLength(50);
                address.Property(a => a.District).HasColumnName("AddressDistrict").HasMaxLength(50);
                address.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(50);
            });
            builder.HasIndex(tp => tp.EmployeeCode).IsUnique();
            builder.HasIndex(tp => tp.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
            builder.HasMany(tp => tp.Slots).WithOne(s => s.Technician).HasForeignKey(s => s.TechnicianId).OnDelete(DeleteBehavior.Cascade);
            builder.Ignore(tp => tp.DomainEvents);
        }
    }
}
