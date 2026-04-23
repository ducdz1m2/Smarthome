using Domain.Entities.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.SenderType)
            .HasConversion<int>();

        builder.HasMany(m => m.Attachments)
            .WithOne(a => a.Message)
            .HasForeignKey(a => a.ChatMessageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tell EF Core to use the private backing field for Attachments
        builder.Metadata.FindNavigation(nameof(ChatMessage.Attachments))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
