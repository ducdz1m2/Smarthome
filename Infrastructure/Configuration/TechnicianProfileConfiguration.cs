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
            builder.Property(tp => tp.BaseSalary).HasPrecision(18, 2);
            builder.Property(tp => tp.EmployeeCode).IsRequired();
            builder.HasIndex(tp => tp.EmployeeCode).IsUnique();
            builder.HasIndex(tp => tp.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
            builder.HasMany(tp => tp.Slots).WithOne(s => s.Technician).HasForeignKey(s => s.TechnicianId).OnDelete(DeleteBehavior.Cascade);
            builder.Ignore(tp => tp.DomainEvents);
        }
    }
}
