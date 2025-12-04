using MediatR;
using System.Collections.Generic;
using WorkSpace.Application.DTOs.Chat;

namespace WorkSpace.Application.Features.CustomerChat.Queries.GetCustomerChatMessages;

public class GetCustomerChatMessagesQuery : IRequest<IEnumerable<CustomerChatMessageDto>>
{
    public string SessionId { get; set; } = string.Empty;
}


