using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class WarrantyClaimConfiguration : IEntityTypeConfiguration<WarrantyClaim>
    {
        public void Configure(EntityTypeBuilder<WarrantyClaim> builder)
        {
            builder.ToTable("WarrantyClaims");
            builder.HasKey(wc => wc.Id);
            builder.Property(wc => wc.Issue).IsRequired().HasMaxLength(500);
            builder.Property(wc => wc.Resolution).HasMaxLength(500);
            builder.HasIndex(wc => wc.WarrantyId);
            builder.HasIndex(wc => wc.TechnicianId);
            builder.Ignore(wc => wc.DomainEvents);
        }
    }
}
