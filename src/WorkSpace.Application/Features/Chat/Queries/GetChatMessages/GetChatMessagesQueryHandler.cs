using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Chat.Queries.GetChatMessages;

public class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, Response<IEnumerable<ChatMessageDto>>>
{
    private readonly IGenericRepositoryAsync<ChatThread> _chatThreadRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    
    public GetChatMessagesQueryHandler(IGenericRepositoryAsync<ChatThread> chatThreadRepository,
        IChatMessageRepository chatMessageRepository)
    {
        _chatThreadRepository = chatThreadRepository;
        _chatMessageRepository = chatMessageRepository;
    }
    public async Task<Response<IEnumerable<ChatMessageDto>>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var thread = await _chatThreadRepository.GetByIdAsync(request.ThreadId, cancellationToken);

        if (thread == null)
        {
            throw new ApiException($"Không tìm thấy cuộc trò chuyện với ID {request.ThreadId}.");
        }
        var isParticipant =
            (thread.CustomerId.HasValue && request.RequestUserId == thread.CustomerId.Value) ||
            (thread.HostUserId.HasValue && request.RequestUserId == thread.HostUserId.Value);

        if (!isParticipant)
        {
            throw new ApiException("Bạn không có quyền truy cập tin nhắn trong cuộc trò chuyện này.");
        }
        
        var message = await _chatMessageRepository.GetMessagesByThreadIdAsync(thread.Id, cancellationToken);

        if (!message.Any())
        {
            return new Response<IEnumerable<ChatMessageDto>>(Enumerable.Empty<ChatMessageDto>());
        }

        var userIds = message
            .Select(m => m.SenderId)
            .Distinct()
            .ToList();
        
        var users = await _chatMessageRepository.GetUsersByIdsAsync(userIds, cancellationToken);
        
        var userMap = users.ToDictionary(u => u.Id, FormatUserName);
        
        var dtos = message.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            ThreadId = m.ThreadId,
            SenderId = m.SenderId,
            SenderName = userMap.TryGetValue(m.SenderId, out var senderName) ? senderName : string.Empty,
            Content = m.Content,
            SentUtc = m.CreateUtc,
            IsRead = m.IsRead,
            ReadUtc = m.ReadAtUtc
        }).ToList();

        return new Response<IEnumerable<ChatMessageDto>>(dtos);
    }
    
    private static string FormatUserName(AppUser user)
    {
        var fullName = $"{user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.UserName ?? string.Empty : fullName;
    }
}