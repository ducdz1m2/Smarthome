using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");
            builder.HasKey(pt => pt.Id);
            builder.Property(pt => pt.Amount).HasPrecision(18, 2);
            builder.Property(pt => pt.TransactionCode).HasMaxLength(100);
            builder.HasIndex(pt => pt.OrderId);
            builder.HasIndex(pt => pt.TransactionCode).IsUnique();
            builder.Ignore(pt => pt.DomainEvents);
        }
    }
}
