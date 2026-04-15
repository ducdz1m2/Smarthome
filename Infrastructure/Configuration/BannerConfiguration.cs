using Domain.Entities.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration
{
    public class BannerConfiguration : IEntityTypeConfiguration<Banner>
    {
        public void Configure(EntityTypeBuilder<Banner> builder)
        {
            builder.ToTable("Banners");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Title).IsRequired().HasMaxLength(200);
            builder.Property(b => b.Subtitle).HasMaxLength(500);
            builder.Property(b => b.Position).IsRequired().HasMaxLength(50);
            
            builder.Property(b => b.ImageUrl).HasConversion(
                url => url.Value,
                value => Domain.ValueObjects.WebsiteUrl.Create(value));
            builder.Property(b => b.LinkUrl).HasConversion(
                url => url != null ? url.Value : null,
                value => value != null ? Domain.ValueObjects.WebsiteUrl.Create(value) : null);
            
            builder.Property(b => b.IsActive).HasDefaultValue(true);
            builder.HasIndex(b => b.Position);
            builder.HasIndex(b => b.IsActive);
            builder.Ignore(b => b.DomainEvents);
        }
    }
}
