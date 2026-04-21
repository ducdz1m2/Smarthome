using Domain.Abstractions;

namespace Domain.Entities.Communication;

public class ChatAttachment : Entity
{
    public int ChatMessageId { get; private set; }
    public ChatMessage Message { get; private set; } = null!;

    public string FileName { get; private set; } = string.Empty;
    public string FileUrl { get; private set; } = string.Empty;
    public string? FileType { get; private set; }
    public long? FileSize { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private ChatAttachment() { }

    public static ChatAttachment Create(int chatMessageId, string fileName, string fileUrl, string? fileType, long? fileSize, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("FileName cannot be empty");
        if (string.IsNullOrWhiteSpace(fileUrl))
            throw new ArgumentException("FileUrl cannot be empty");

        return new ChatAttachment
        {
            ChatMessageId = chatMessageId,
            FileName = fileName.Trim(),
            FileUrl = fileUrl.Trim(),
            FileType = fileType,
            FileSize = fileSize,
            UploadedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }
}
