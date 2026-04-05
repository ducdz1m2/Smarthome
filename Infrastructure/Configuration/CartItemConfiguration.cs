using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.ToTable("CartItems");
            builder.HasKey(ci => ci.Id);
            builder.HasIndex(ci => ci.UserId);
            builder.HasIndex(ci => new { ci.UserId, ci.ProductId, ci.VariantId }).IsUnique();
            builder.Ignore(ci => ci.DomainEvents);
        }
    }
}
