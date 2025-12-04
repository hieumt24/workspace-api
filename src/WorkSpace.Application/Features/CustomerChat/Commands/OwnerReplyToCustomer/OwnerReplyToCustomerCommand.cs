using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.CustomerChat.Commands.OwnerReplyToCustomer;

public class OwnerReplyToCustomerCommand : IRequest<Response<CustomerChatMessageDto>>
{
    public string SessionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int OwnerUserId { get; set; }
}


