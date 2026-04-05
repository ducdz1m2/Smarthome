using Domain.Entities.Installation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class InstallationSlotConfiguration : IEntityTypeConfiguration<InstallationSlot>
    {
        public void Configure(EntityTypeBuilder<InstallationSlot> builder)
        {
            builder.ToTable("InstallationSlots");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.IsBooked).HasDefaultValue(false);
            builder.HasIndex(s => s.TechnicianId);
            builder.HasIndex(s => s.Date);
            builder.HasIndex(s => new { s.TechnicianId, s.Date, s.IsBooked });
            builder.Ignore(s => s.DomainEvents);
        }
    }
}
