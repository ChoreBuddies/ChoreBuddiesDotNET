using ChoreBuddies.Backend.Domain;
using ChoreBuddies.Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace ChoreBuddies.Backend.Features.Chat;

public interface IChatRepository
{
    Task<ChatMessage?> CreateChatMessageAsync(ChatMessage newMessage);
    Task<List<ChatMessage>> GetNewestMessagesAsync(int householdId, int numberOfMessages);
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

    public async Task<List<ChatMessage>> GetNewestMessagesAsync(int householdId, int numberOfMessages)
    {
        var messages = _dbContext.ChatMessages
            .Where(m => m.HouseholdId == householdId)
            .OrderByDescending(m => m.SentAt)
            .Take(50)
            .Include(m => m.Sender);

        return await messages.ToListAsync();
    }
}
