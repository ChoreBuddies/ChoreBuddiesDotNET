using Shared.Chat;

namespace ChoreBuddies.Frontend.Features.Chat;

public enum MessageStatus
{
    None,
    Sending,
    Sent
}

public class ChatMessageVm
{
    public int Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsMine { get; set; }
    public DateTimeOffset SentAt { get; set; }
    public MessageStatus Status { get; set; } = MessageStatus.None;
    public Guid? ClientUniqueId { get; set; }

    public static ChatMessageVm FromDto(ChatMessageDto dto)
    {
        return new ChatMessageVm
        {
            Id = dto.Id,
            SenderName = dto.SenderName,
            Content = dto.Content,
            IsMine = dto.IsMine,
            SentAt = dto.SentAt,
            Status = MessageStatus.None,
            ClientUniqueId = dto.ClientUniqueId
        };
    }
}
