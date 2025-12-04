using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.CustomerChat.Commands.StartCustomerChat;

public class StartCustomerChatCommandHandler : IRequestHandler<StartCustomerChatCommand, Response<CustomerChatSessionDto>>
{
    private readonly ICustomerChatSessionRepository _sessionRepository;
    private readonly IGenericRepositoryAsync<CustomerChatMessage> _messageRepository;
    private readonly IGenericRepositoryAsync<AppUser> _userRepository;
    private readonly IDateTimeService _dateTimeService;

    public StartCustomerChatCommandHandler(
        ICustomerChatSessionRepository sessionRepository,
        IGenericRepositoryAsync<CustomerChatMessage> messageRepository,
        IGenericRepositoryAsync<AppUser> userRepository,
        IDateTimeService dateTimeService)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _dateTimeService = dateTimeService;
    }
    public async Task<Response<CustomerChatSessionDto>> Handle(StartCustomerChatCommand request, CancellationToken cancellationToken)
    {
         var now = _dateTimeService.NowUtc;

         // Get customer info from database
         var customer = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
         if (customer == null)
         {
             throw new ApiException("Customer not found");
         }

         var session = new CustomerChatSession()
         {
            SessionId = Guid.NewGuid().ToString(),
            CustomerId = customer.Id,
            CustomerName = customer.GetFullName(),
            CustomerEmail = customer.Email,
            IsActive = true,
            CreatedById = customer.Id,
            CreateUtc = now,
            LastMessageAt = now,
         };
         
         await _sessionRepository.AddAsync(session, cancellationToken);
         
         // Send initial message if provided
         if (!string.IsNullOrWhiteSpace(request.RequestDto.InitialMessage))
         {
             var initialMessage = new CustomerChatMessage
             {
                 CustomerChatSessionId = session.Id,
                 Content = request.RequestDto.InitialMessage.Trim(),
                 SenderName = session.CustomerName,
                 IsOwner = false,
                 OwnerId = null,
                 CreatedById = customer.Id,
                 CreateUtc = now
             };
             
             await _messageRepository.AddAsync(initialMessage, cancellationToken);
         }
         
         var dto = new CustomerChatSessionDto
         {
             SessionId = session.SessionId,
             CustomerName = session.CustomerName,
             CustomerEmail = session.CustomerEmail,
             CreatedAt = session.CreateUtc,
             LastMessageAt = session.LastMessageAt,
             IsActive = session.IsActive
         };

         return new Response<CustomerChatSessionDto>(dto, "Customer chat session started successfully");
    }
}


