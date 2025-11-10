using FluentValidation;

namespace WorkSpace.Application.Features.BookingStatus.Commands.CreateBookingStatus;

public class CreateBookingStatusCommandValidator : AbstractValidator<CreateBookingStatusCommand>
{
    public CreateBookingStatusCommandValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull()
            .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.");

        RuleFor(p => p.Description)
            .MaximumLength(255).WithMessage("{PropertyName} must not exceed 255 characters.");
    }
}

