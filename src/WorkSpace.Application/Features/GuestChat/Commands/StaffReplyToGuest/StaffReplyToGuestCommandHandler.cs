using MediatR;
using Microsoft.AspNetCore.Identity;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.GuestChat.Commands.StaffReplyToGuest;

public class StaffReplyToGuestCommandHandler : IRequestHandler<StaffReplyToGuestCommand, Response<GuestChatMessageDto>>
{
    private readonly IGuestChatSessionRepository _sessionRepository;
    private readonly IGenericRepositoryAsync<GuestChatMessage> _messageRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly UserManager<AppUser> _userManager;

    public StaffReplyToGuestCommandHandler(
        IGuestChatSessionRepository sessionRepository,
        IGenericRepositoryAsync<GuestChatMessage> messageRepository,
        IDateTimeService dateTimeService,
        UserManager<AppUser> userManager)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _dateTimeService = dateTimeService;
        _userManager = userManager;
    }
    public async Task<Response<GuestChatMessageDto>> Handle(StaffReplyToGuestCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
            
        if (session == null)
        {
            throw new ApiException($"Guest chat session not found: {request.SessionId}");
        }

        var staff = await _userManager.FindByIdAsync(request.StaffUserId.ToString());
        if (staff == null)
        {
            throw new ApiException("Staff user not found");
        }

        var staffName = GetStaffName(staff);
        var now = _dateTimeService.NowUtc;

        if (session.AssignedStaffId == null)
        {
            session.AssignedStaffId = request.StaffUserId;
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }

        var message = new GuestChatMessage
        {
            GuestChatSessionId = session.Id,
            Content = request.Message,
            SenderName = staffName,
            IsStaff = true,
            StaffId = request.StaffUserId,
            CreateUtc = now
        };

        await _messageRepository.AddAsync(message, cancellationToken);

        session.LastMessageAt = now;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        var dto = new GuestChatMessageDto
        {
            Id = message.Id,
            SessionId = session.SessionId,
            SenderName = message.SenderName,
            IsStaff = message.IsStaff,
            Content = message.Content,
            SentAt = message.CreateUtc
        };

        return new Response<GuestChatMessageDto>(dto, "Staff reply sent successfully");
    }
    
    private static string GetStaffName(AppUser user)
    {
        var fullName = $"{user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.UserName ?? "Staff" : fullName;
    }
}