namespace Shared.Chat;

public record ChatMessageDto(
    int Id,
    string SenderName,
    string Content,
    DateTimeOffset SentAt,
    bool IsMine,
    Guid? ClientUniqueId = null
);
