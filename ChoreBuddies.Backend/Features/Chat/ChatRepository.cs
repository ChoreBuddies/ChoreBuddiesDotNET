using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace ChoreBuddies.Backend.Features.Chat;

public interface IChatRepository
{
    Task<ChatMessage?> CreateChatMessageAsync(ChatMessage newMessage);
    Task<List<ChatMessage>> GetMessagesAsync(int householdId, int numberOfMessages, DateTimeOffset? beforeDate);
}

public class ChatRepository(ChoreBuddiesDbContext dbContext) : IChatRepository
{
    private readonly ChoreBuddiesDbContext _dbContext = dbContext;

    public async Task<ChatMessage?> CreateChatMessageAsync(ChatMessage newMessage)
    {
        var message = _dbContext.ChatMessages.Add(newMessage);
        await _dbContext.SaveChangesAsync();
        return message.Entity;
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(int householdId, int numberOfMessages, DateTimeOffset? beforeDate)
    {
        var query = _dbContext.ChatMessages
            .Where(m => m.HouseholdId == householdId);

        if (beforeDate.HasValue)
        {
            // Pobierz wiadomości starsze niż podana data
            query = query.Where(m => m.SentAt < beforeDate.Value);
        }

        var messages = await query
            .OrderByDescending(m => m.SentAt) // Najpierw najnowsze (od daty odcięcia w dół)
            .Take(numberOfMessages)
            .Include(m => m.Sender)
            .ToListAsync();

        return messages;
    }
}
