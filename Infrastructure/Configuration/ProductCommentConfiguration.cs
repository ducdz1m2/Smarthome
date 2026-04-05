using Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class ProductCommentConfiguration : IEntityTypeConfiguration<ProductComment>
    {
        public void Configure(EntityTypeBuilder<ProductComment> builder)
        {
            builder.ToTable("ProductComments");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.Content).IsRequired().HasMaxLength(1000);
            builder.Property(c => c.IsApproved).HasDefaultValue(false);
            builder.Property(c => c.IsVerifiedPurchase).HasDefaultValue(false);
            builder.HasIndex(c => c.ProductId);
            builder.HasIndex(c => c.UserId);
            builder.HasIndex(c => c.IsApproved);
            builder.HasOne(c => c.Product).WithMany(p => p.Comments).HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(c => c.ParentComment).WithMany(c => c.Replies).HasForeignKey(c => c.ParentCommentId).OnDelete(DeleteBehavior.Restrict);
            builder.Ignore(c => c.DomainEvents);
        }
    }
}
