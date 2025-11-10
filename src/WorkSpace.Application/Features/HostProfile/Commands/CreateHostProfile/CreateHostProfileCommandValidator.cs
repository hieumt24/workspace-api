using FluentValidation;

namespace WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;

public class CreateHostProfileCommandValidator : AbstractValidator<CreateHostProfileCommand>
{
    public CreateHostProfileCommandValidator()
    {
        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage("{PropertyName} is required.")
            .NotNull();

        RuleFor(p => p.CompanyName)
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.");

        RuleFor(p => p.Description)
            .MaximumLength(500).WithMessage("{PropertyName} must not exceed 500 characters.");

        RuleFor(p => p.ContactPhone)
            .MaximumLength(15).WithMessage("{PropertyName} must not exceed 15 characters.");

        RuleFor(p => p.LogoUrl)
            .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.");

        RuleFor(p => p.WebsiteUrl)
            .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.");
    }
    
}