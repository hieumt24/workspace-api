using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface IChatbotConversationRepository
{
    Task<ChatbotConversation?> GetChatbotConversationAsync(int userId, CancellationToken cancellationToken = default);
    Task<ChatbotConversation?> GetChatbotConversationWithMessagesAsync(int conversationId, CancellationToken cancellationToken = default);

    Task<ChatbotConversationMessage> AddMessageAsync(
        int conversationId,
        string role,
        string content,
        string? extractedIntentJson = null,
        int? recommendationCount = null,
        CancellationToken cancellationToken = default);
    
    Task<List<ChatbotConversationMessage>> GetRecentMessagesAsync(
        int conversationId, 
        int count = 10, 
        CancellationToken cancellationToken = default);

    Task<List<ChatbotConversation>> GetUserConversationsAsync(
        int userId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<ChatbotConversation> StartNewConversationAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task DeleteConversationAsync(
        int conversationId,
        CancellationToken cancellationToken = default);

    Task UpdateConversationTitleAsync(
        int conversationId,
        string title,
        CancellationToken cancellationToken = default);
}