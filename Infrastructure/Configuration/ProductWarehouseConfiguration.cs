using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ProductWarehouseConfiguration : IEntityTypeConfiguration<ProductWarehouse>
    {
        public void Configure(EntityTypeBuilder<ProductWarehouse> builder)
        {
            builder.ToTable("ProductWarehouses");
            builder.HasKey(pw => pw.Id);
            builder.HasIndex(pw => new { pw.ProductId, pw.WarehouseId }).IsUnique();
            builder.HasOne(pw => pw.Warehouse).WithMany(w => w.ProductWarehouses).HasForeignKey(pw => pw.WarehouseId).OnDelete(DeleteBehavior.Cascade);
            builder.Ignore(pw => pw.DomainEvents);
        }
    }
}
