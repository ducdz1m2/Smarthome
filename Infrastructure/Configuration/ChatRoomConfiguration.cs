using Domain.Entities.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.ToTable("ChatRooms");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Type)
            .HasConversion<int>();

        builder.HasMany(r => r.Participants)
            .WithOne(p => p.ChatRoom)
            .HasForeignKey(p => p.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Messages)
            .WithOne(m => m.ChatRoom)
            .HasForeignKey(m => m.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Tell EF Core to use the private backing fields for the collections
        builder.Metadata.FindNavigation(nameof(ChatRoom.Participants))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
            
        builder.Metadata.FindNavigation(nameof(ChatRoom.Messages))?
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
