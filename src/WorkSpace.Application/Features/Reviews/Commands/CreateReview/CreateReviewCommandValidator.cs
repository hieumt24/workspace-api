using FluentValidation;

namespace WorkSpace.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
    {
        public CreateReviewCommandValidator()
        {
            RuleFor(p => p.UserId)
                .GreaterThan(0).WithMessage("User ID is required.");

            RuleFor(p => p.BookingId) 
                .GreaterThan(0).WithMessage("Booking ID is required.");

           
            RuleFor(p => p.Dto.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");

            RuleFor(p => p.Dto.Comment)
                .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
        }
    }
}