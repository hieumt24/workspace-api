using MediatR;
using Microsoft.AspNetCore.Identity;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Chat.Commands.SendChatMessage;

public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, Response<ChatMessageDto>>
{
    private readonly IGenericRepositoryAsync<ChatThread> _chatThreadRepository;
    private readonly IGenericRepositoryAsync<ChatMessage> _chatMessageRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly UserManager<AppUser> _userManager;

    public SendChatMessageCommandHandler(IGenericRepositoryAsync<ChatThread> chatThreadRepository, 
        IDateTimeService dateTimeService,
        UserManager<AppUser> userManager,
        IGenericRepositoryAsync<ChatMessage> chatMessageRepository)
    {
        _chatThreadRepository = chatThreadRepository;
        _dateTimeService = dateTimeService;
        _userManager = userManager;
        _chatMessageRepository = chatMessageRepository;
        
    }
    public async Task<Response<ChatMessageDto>> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        var thread = await _chatThreadRepository.GetByIdAsync(request.RequestDto.ThreadId, cancellationToken);
        if (thread == null)
        {
            throw new ApiException($"Không tìm thấy cuộc trò chuyện với ID {request.RequestDto.ThreadId}.");
        }

        var isParticipant = (thread.CustomerId.HasValue && request.SenderId == thread.CustomerId.Value) ||
                            (thread.HostUserId.HasValue && request.SenderId == thread.HostUserId.Value);
        if (!isParticipant)
        {
            throw new ApiException("Bạn không có quyền gửi tin nhắn trong cuộc trò chuyện này.");
        }

        var now = _dateTimeService.NowUtc;
        
        var chatMessage = new ChatMessage
        {
            ThreadId = request.RequestDto.ThreadId,
            SenderId = request.SenderId,
            Content = request.RequestDto.Content.Trim(),
            IsRead = false,
            CreatedById = request.SenderId,
            CreateUtc = now
        };
        
        await _chatMessageRepository.AddAsync(chatMessage,cancellationToken);
        
        thread.LastMessageUtc = _dateTimeService.NowUtc;
        thread.LastMessagePreview = chatMessage.Content.Length > 500 ? chatMessage.Content.Substring(0, 500) : chatMessage.Content;
        thread.LastModifiedUtc = _dateTimeService.NowUtc;
        thread.LastModifiedById = request.SenderId;
        thread.HasUnreadMessages = true;
        await _chatThreadRepository.UpdateAsync(thread,cancellationToken);
        
        var sender = await _userManager.FindByIdAsync(request.SenderId.ToString());
        var dto = new ChatMessageDto()
        {
            Id = chatMessage.Id,
            ThreadId = chatMessage.ThreadId,
            SenderId = chatMessage.SenderId,
            SenderName = FormatUserName(sender),
            Content = chatMessage.Content,
            SentUtc = chatMessage.CreateUtc,
            IsRead = chatMessage.IsRead,
            ReadUtc = chatMessage.ReadAtUtc
        };
        return new Response<ChatMessageDto>(dto, message: "Gửi tin nhắn thành công.");
    }
    
    private static string FormatUserName(AppUser? user)
    {
        if (user == null)
        {
            return string.Empty;
        }

        var fullName = $"{user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.UserName ?? string.Empty : fullName;
    }
}
