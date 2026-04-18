using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class StockIssueDetailConfiguration : IEntityTypeConfiguration<StockIssueDetail>
{
    public void Configure(EntityTypeBuilder<StockIssueDetail> builder)
    {
        builder.ToTable("StockIssueDetails");
        builder.HasKey(sid => sid.Id);
        builder.HasIndex(sid => sid.StockIssueId);
        builder.HasIndex(sid => sid.ProductId);

        // Navigation properties removed to prevent tracking conflicts
        // No HasOne configuration needed

        builder.Ignore(sid => sid.DomainEvents);
    }
}
