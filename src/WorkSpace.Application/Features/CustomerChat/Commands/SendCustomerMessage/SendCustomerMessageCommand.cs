using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.CustomerChat.Commands.SendCustomerMessage;

public class SendCustomerMessageCommand : IRequest<Response<CustomerChatMessageDto>>
{
    public SendCustomerMessageRequestDto RequestDto { get; set; } = null!;
}


