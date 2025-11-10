using MediatR;
using WorkSpace.Application.DTOs.Support;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Enums; 

namespace WorkSpace.Application.Features.SupportTickets.Commands
{

    public class CreateTicketCommand : IRequest<Response<int>>
    {
        public CreateTicketRequest Dto { get; set; }


        public int SubmittedByUserId { get; set; }
    }
}