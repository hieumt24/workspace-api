using MediatR;
using Microsoft.AspNetCore.Identity;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.CustomerChat.Commands.OwnerReplyToCustomer;

public class OwnerReplyToCustomerCommandHandler : IRequestHandler<OwnerReplyToCustomerCommand, Response<CustomerChatMessageDto>>
{
    private readonly ICustomerChatSessionRepository _sessionRepository;
    private readonly IGenericRepositoryAsync<CustomerChatMessage> _messageRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly UserManager<AppUser> _userManager;

    public OwnerReplyToCustomerCommandHandler(
        ICustomerChatSessionRepository sessionRepository,
        IGenericRepositoryAsync<CustomerChatMessage> messageRepository,
        IDateTimeService dateTimeService,
        UserManager<AppUser> userManager)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _dateTimeService = dateTimeService;
        _userManager = userManager;
    }
    public async Task<Response<CustomerChatMessageDto>> Handle(OwnerReplyToCustomerCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
            
        if (session == null)
        {
            throw new ApiException($"Customer chat session not found: {request.SessionId}");
        }

        var owner = await _userManager.FindByIdAsync(request.OwnerUserId.ToString());
        if (owner == null)
        {
            throw new ApiException("Owner user not found");
        }

        var ownerName = GetOwnerName(owner);
        var now = _dateTimeService.NowUtc;

        if (session.AssignedOwnerId == null)
        {
            session.AssignedOwnerId = request.OwnerUserId;
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }

        var message = new CustomerChatMessage
        {
            CustomerChatSessionId = session.Id,
            Content = request.Message,
            SenderName = ownerName,
            IsOwner = true,
            OwnerId = request.OwnerUserId,
            CreateUtc = now
        };

        await _messageRepository.AddAsync(message, cancellationToken);

        session.LastMessageAt = now;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        var dto = new CustomerChatMessageDto
        {
            Id = message.Id,
            SessionId = session.SessionId,
            SenderName = message.SenderName,
            IsOwner = message.IsOwner,
            Content = message.Content,
            SentAt = message.CreateUtc
        };

        return new Response<CustomerChatMessageDto>(dto, "Owner reply sent successfully");
    }
    
    private static string GetOwnerName(AppUser user)
    {
        var fullName = $"{user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.UserName ?? "Owner" : fullName;
    }
}
