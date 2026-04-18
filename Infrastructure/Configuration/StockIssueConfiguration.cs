using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class StockIssueConfiguration : IEntityTypeConfiguration<StockIssue>
{
    public void Configure(EntityTypeBuilder<StockIssue> builder)
    {
        builder.ToTable("StockIssues");
        builder.HasKey(si => si.Id);
        builder.HasIndex(si => si.WarehouseId);
        builder.HasIndex(si => si.BookingId);

        // Navigation properties removed to prevent tracking conflicts
        // No HasOne configuration needed

        builder.Ignore(si => si.DomainEvents);
    }
}
