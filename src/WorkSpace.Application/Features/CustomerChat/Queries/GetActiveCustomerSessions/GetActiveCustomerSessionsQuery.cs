using MediatR;
using System.Collections.Generic;
using WorkSpace.Application.DTOs.Chat;


namespace WorkSpace.Application.Features.CustomerChat.Queries.GetActiveCustomerSessions;


public class GetActiveCustomerSessionsQuery : IRequest<IEnumerable<CustomerChatSessionDto>>
{
    public int? OwnerId { get; set; }
    
}


