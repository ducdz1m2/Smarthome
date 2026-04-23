using Domain.Entities.Communication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configuration;

public class ChatParticipantConfiguration : IEntityTypeConfiguration<ChatParticipant>
{
    public void Configure(EntityTypeBuilder<ChatParticipant> builder)
    {
        builder.ToTable("ChatParticipants");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserType)
            .HasConversion<int>();
            
        builder.HasOne(p => p.ChatRoom)
            .WithMany(r => r.Participants)
            .HasForeignKey(p => p.ChatRoomId);
    }
}
