using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Chat.Commands.StartChatThread;

public class StartChatThreadCommandHandler : IRequestHandler<StartChatThreadCommand, Response<ChatThreadDto>>
{
    private readonly IDateTimeService _dateTimeService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IGenericRepositoryAsync<ChatThread> _chatThreadRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IApplicationDbContext _context;

    public StartChatThreadCommandHandler(IDateTimeService dateTimeService, 
        IBookingRepository bookingRepository,
        IGenericRepositoryAsync<ChatThread> chatThreadRepository,
        IChatMessageRepository chatMessageRepository,
        IApplicationDbContext context)
    {
        _dateTimeService = dateTimeService;
        _bookingRepository = bookingRepository;
        _chatThreadRepository = chatThreadRepository;
        _chatMessageRepository = chatMessageRepository;
        _context = context;
    }
    public async Task<Response<ChatThreadDto>> Handle(StartChatThreadCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetBookingWithDetailsAsync(request.RequestDto.BookingId, cancellationToken);

        if (booking == null)
        {
            throw new ApiException($"Không tìm thấy đặt chỗ với ID {request.RequestDto.BookingId}.");
        }

        var hostUserId = booking.WorkSpaceRoom?.WorkSpace?.Host?.UserId;

        if (hostUserId == null)
        {
            throw new ApiException($"Không tìm thấy chủ sở hữu cho không gian làm việc trong đặt chỗ ID {request.RequestDto.BookingId}.");
        }
        
        var isParticipant =
            request.RequestUserId == booking.CustomerId ||
            request.RequestUserId == hostUserId.Value;

        if (!isParticipant)
        {
            throw new ApiException("Bạn không có quyền bắt đầu cuộc trò chuyện cho đặt chỗ này.");
        }
        
    

        var existingThread = await _context.ChatThreads.FirstOrDefaultAsync(
            t => t.BookingId == booking.Id
            && t.CustomerId == booking.CustomerId 
            && t.HostUserId == hostUserId,
            cancellationToken);
        if (existingThread != null)
        {
            bool hasUnreadMessage = await _chatMessageRepository.CheckHasUnreadMessagesAsync(existingThread.Id, request.RequestUserId, cancellationToken);

            var dtoExisting = new ChatThreadDto()
            {
                Id = existingThread.Id,
                BookingId = existingThread.BookingId ?? 0,
                CustomerId = existingThread.CustomerId ?? 0,
                CustomerName = FormatUserName(booking.Customer),
                HostUserId = existingThread.HostUserId ?? 0,
                HostName = FormatUserName(booking.WorkSpaceRoom?.WorkSpace?.Host?.User),
                CreatedUtc = existingThread.CreateUtc,
                LastMessageUtc = existingThread.LastMessageUtc,
                LastMessagePreview = existingThread.LastMessagePreview,
                HasUnreadMessages = hasUnreadMessage
            };
            
            return new Response<ChatThreadDto>(dtoExisting, "Cuộc trò chuyện đã tồn tại.");
        }

        var now = _dateTimeService.NowUtc;
        var chatThread = new Domain.Entities.ChatThread
        {
            BookingId = request.RequestDto.BookingId,
            CustomerId = booking.CustomerId,
            HostUserId = hostUserId.Value,
            CreatedById = request.RequestUserId,
            CreateUtc = _dateTimeService.NowUtc,
            LastModifiedById = request.RequestUserId,
            LastModifiedUtc = now
        };
        await _chatThreadRepository.AddAsync(chatThread, cancellationToken);
        
        
        var hasUnreadMessages = await _chatMessageRepository.CheckHasUnreadMessagesAsync(chatThread.Id, request.RequestUserId, cancellationToken);

        var dto = new ChatThreadDto()
        {
            Id = chatThread.Id,
            BookingId = chatThread.BookingId ?? 0,
            CustomerId = chatThread.CustomerId ?? 0,
            CustomerName = FormatUserName(booking.Customer),
            HostUserId = chatThread.HostUserId ?? 0,
            HostName = FormatUserName(booking.WorkSpaceRoom?.WorkSpace?.Host?.User),
            CreatedUtc = chatThread.CreateUtc,
            LastMessageUtc = chatThread.LastMessageUtc,
            LastMessagePreview = chatThread.LastMessagePreview,
            HasUnreadMessages = hasUnreadMessages
        };
        
        return new Response<ChatThreadDto>(dto, "Cuộc trò chuyện đã được bắt đầu thành công.");
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