using Domain.Entities.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class ChatAttachmentConfiguration : IEntityTypeConfiguration<ChatAttachment>
{
    public void Configure(EntityTypeBuilder<ChatAttachment> builder)
    {
        builder.ToTable("ChatAttachment");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.FileUrl)
            .IsRequired();

        // Relationship with Message is configured in ChatMessageConfiguration
    }
}
