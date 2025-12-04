using MediatR;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.CustomerChat.Commands.StartCustomerChat;

public class StartCustomerChatCommand : IRequest<Response<CustomerChatSessionDto>>
{
    public StartCustomerChatRequestDto RequestDto { get; set; } = null!;
    public int UserId { get; set; }
}


