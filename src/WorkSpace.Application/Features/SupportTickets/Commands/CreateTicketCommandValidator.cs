using FluentValidation;
using WorkSpace.Domain.Enums;

namespace WorkSpace.Application.Features.SupportTickets.Commands
{
    public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
    {
        public CreateTicketCommandValidator()
        {
            RuleFor(p => p.SubmittedByUserId)
                .GreaterThan(0).WithMessage("User ID không hợp lệ.");

            RuleFor(p => p.Dto.Subject)
                .NotEmpty().WithMessage("Chủ đề không được để trống.")
                .MaximumLength(255).WithMessage("Chủ đề không quá 255 ký tự.");

            RuleFor(p => p.Dto.Message)
                .NotEmpty().WithMessage("Nội dung không được để trống.");

            RuleFor(p => p.Dto.TicketType)
                .IsInEnum().WithMessage("Loại phiếu không hợp lệ.");
        }
    }
}