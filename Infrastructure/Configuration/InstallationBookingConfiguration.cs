using Domain.Entities.Installation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class InstallationBookingConfiguration : IEntityTypeConfiguration<InstallationBooking>
    {
        public void Configure(EntityTypeBuilder<InstallationBooking> builder)
        {
            builder.ToTable("InstallationBookings");
            builder.HasKey(ib => ib.Id);
            builder.Property(ib => ib.CustomerSignature).HasMaxLength(200);
            builder.Property(ib => ib.Notes).HasMaxLength(1000);
            builder.HasIndex(ib => ib.OrderId);
            builder.HasIndex(ib => ib.TechnicianId);
            builder.HasIndex(ib => ib.SlotId);
            builder.HasIndex(ib => ib.ScheduledDate);

            builder.HasOne(ib => ib.Technician)
                .WithMany()
                .HasForeignKey(ib => ib.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ib => ib.Slot)
                .WithMany()
                .HasForeignKey(ib => ib.SlotId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            
            builder.HasMany(ib => ib.Materials).WithOne(im => im.Booking).HasForeignKey(im => im.BookingId).OnDelete(DeleteBehavior.Cascade);
            builder.Ignore(ib => ib.DomainEvents);
        }
    }
}
