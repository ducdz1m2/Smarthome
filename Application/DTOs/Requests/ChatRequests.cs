namespace Application.DTOs.Requests;

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public List<ChatAttachmentRequest>? Attachments { get; set; }
}

public class ChatAttachmentRequest
{
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
}

public class EditMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

public class CreateOneToOneChatRequest
{
    public int Participant1Id { get; set; }
    public string Participant1Type { get; set; } = string.Empty; // Customer, Admin, Technician
    public int Participant2Id { get; set; }
    public string Participant2Type { get; set; } = string.Empty;
    public string? Title { get; set; }
}

public class CreateSupportChatRequest
{
    public int CustomerId { get; set; }
    public int? OrderId { get; set; }
    public int? InstallationId { get; set; }
    public int? WarrantyClaimId { get; set; }
    public string? InitialMessage { get; set; }
}

public class JoinChatRequest
{
    public int UserId { get; set; }
    public string UserType { get; set; } = string.Empty;
}
