using ChoreBuddies.Backend.Domain;
using Shared.Chat;

namespace ChoreBuddies.Backend.Features.Chat;

public interface IChatService
{
    Task<ChatMessageDto?> CreateChatMessageAsync(AppUser user, string messageContent, Guid clientUniqueId);
    Task<ChatMessageDto?> CreateChatMessageAsync(int userId, int householdId, string userName, string messageContent, Guid clientUniqueId);

    Task<List<ChatMessageDto>> GetNewestMessagesAsync(int userId, int householdId, int numberOfMessages = 50);

    string GetGroupName(int householdId);
}

public class ChatService(IChatRepository chatRepository, TimeProvider timeProvider) : IChatService
{
    private readonly IChatRepository _chatRepository = chatRepository;
    private readonly TimeProvider _timeProvider = timeProvider;

    public string GetGroupName(int householdId) => $"Household_{householdId}";

    public async Task<ChatMessageDto?> CreateChatMessageAsync(AppUser user, string messageContent, Guid clientUniqueId)
    {
        return await CreateChatMessageAsync(user.Id, user.HouseholdId!.Value, user.UserName!, messageContent, clientUniqueId);
    }

    public async Task<ChatMessageDto?> CreateChatMessageAsync(int userId, int householdId, string userName, string messageContent, Guid clientUniqueId)
    {
        // Save to database
        var newMessage = new ChatMessage(userId, householdId, messageContent, _timeProvider.GetUtcNow());

        var message = await _chatRepository.CreateChatMessageAsync(newMessage);

        if (message == null)
            return null;

        // Return DTO to group
        return new ChatMessageDto
        (
            message.Id,
            userName,
            message.Content,
            message.SentAt,
            false, // default
            clientUniqueId
        );
    }

    public async Task<List<ChatMessageDto>> GetNewestMessagesAsync(int userId, int householdId, int numberOfMessages = 50)
    {
        var messages = await _chatRepository.GetNewestMessagesAsync(householdId, numberOfMessages);

        return messages.Select(m => new ChatMessageDto(
                m.Id,
                (m.Sender != null && m.Sender.UserName != null) ? m.Sender.UserName : "Unknown",
                m.Content,
                m.SentAt,
                m.SenderId == userId,
                null
            )).ToList();
    }
}
