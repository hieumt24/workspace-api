using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class ChatbotConversationRepository : IChatbotConversationRepository
{
    private readonly WorkSpaceContext _context;
    public ChatbotConversationRepository(WorkSpaceContext context)
    {
        _context = context;
    }
    public async Task<ChatbotConversation?> GetChatbotConversationAsync(int userId, CancellationToken cancellationToken = default)
    {
        var activeConversation = await _context.ChatbotConversations
            .Include(c => c.Messages.OrderBy(m => m.CreateUtc))
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsActive, cancellationToken);
        if (activeConversation != null)
        {
            return activeConversation;
        }

        var newConversation = new ChatbotConversation()
        {
            UserId = userId,
            IsActive = true,
            Title = "New Conversation",
            LastMessageAt = DateTime.UtcNow,
            MessageCount = 0
        };

        await _context.ChatbotConversations.AddAsync(newConversation);
        await _context.SaveChangesAsync(cancellationToken);
        
        return newConversation;

    }

    public async Task<ChatbotConversation?> GetChatbotConversationWithMessagesAsync(int conversationId, CancellationToken cancellationToken = default)
    {
        return await _context.ChatbotConversations
            .Include(c => c.Messages.OrderBy(m => m.CreateUtc))
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);
    }

    public async Task<ChatbotConversationMessage> AddMessageAsync(int conversationId, string role, string content, string? extractedIntentJson = null,
        int? recommendationCount = null, CancellationToken cancellationToken = default)
    {
        var message = new ChatbotConversationMessage()
        {
            ConversationId = conversationId,
            Role = role,
            Content = content,
            ExtractedIntentJson = extractedIntentJson,
            RecommendationCount = recommendationCount,
            CreateUtc = DateTime.UtcNow
        };

        _context.ChatbotConversationMessages.Add(message);
        
        var conversation = await _context.ChatbotConversations
            .FindAsync(new object[] { conversationId }, cancellationToken);
        if (conversation != null)
        {
            conversation.LastMessageAt = DateTime.UtcNow;
            conversation.MessageCount += 1;
        }
        await _context.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<List<ChatbotConversationMessage>> GetRecentMessagesAsync(
        int conversationId,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        return await _context.ChatbotConversationMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreateUtc)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ChatbotConversation>> GetUserConversationsAsync(
        int userId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _context.ChatbotConversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.LastMessageAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<ChatbotConversation> StartNewConversationAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var currentActive = await _context.ChatbotConversations
            .Where(c => c.UserId == userId && c.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var conv in currentActive)
        {
            conv.IsActive = false;
        }


        var newConversation = new ChatbotConversation
        {
            UserId = userId,
            IsActive = true,
            Title = "New Conversation",
            LastMessageAt = DateTime.UtcNow,
            MessageCount = 0
        };

        _context.ChatbotConversations.Add(newConversation);
        await _context.SaveChangesAsync(cancellationToken);

        return newConversation;
    }

    public async Task DeleteConversationAsync(
        int conversationId,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _context.ChatbotConversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

        if (conversation != null)
        {
            _context.ChatbotConversations.Remove(conversation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateConversationTitleAsync(
        int conversationId,
        string title,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _context.ChatbotConversations
            .FindAsync(new object[] { conversationId }, cancellationToken);

        if (conversation != null)
        {
            conversation.Title = title;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}