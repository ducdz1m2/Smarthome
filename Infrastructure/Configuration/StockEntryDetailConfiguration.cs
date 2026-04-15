using Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class StockEntryDetailConfiguration : IEntityTypeConfiguration<StockEntryDetail>
    {
        public void Configure(EntityTypeBuilder<StockEntryDetail> builder)
        {
            builder.ToTable("StockEntryDetails");
            builder.HasKey(sed => sed.Id);
            builder.Property(sed => sed.UnitCost).HasConversion(
                money => money.Amount,
                value => Domain.ValueObjects.Money.Vnd(value));
            builder.HasIndex(sed => sed.StockEntryId);
            builder.HasIndex(sed => sed.ProductId);
            builder.Ignore(sed => sed.DomainEvents);
        }
    }
}
