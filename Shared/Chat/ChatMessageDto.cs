namespace Shared.Chat;

public record ChatMessageDto(
    int Id,
    string SenderName,
    string Content,
    bool IsMine,
    DateTimeOffset SentAt
);
