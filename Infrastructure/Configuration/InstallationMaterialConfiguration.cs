using Domain.Entities.Installation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class InstallationMaterialConfiguration : IEntityTypeConfiguration<InstallationMaterial>
    {
        public void Configure(EntityTypeBuilder<InstallationMaterial> builder)
        {
            builder.ToTable("InstallationMaterials");
            builder.HasKey(im => im.Id);
            builder.HasIndex(im => im.BookingId);
            builder.HasIndex(im => im.ProductId);
            builder.HasIndex(im => im.WarehouseId);

            // Configure relationship with Warehouse
            builder.HasOne(im => im.Warehouse)
                .WithMany()
                .HasForeignKey(im => im.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Ignore(im => im.DomainEvents);
        }
    }
}
