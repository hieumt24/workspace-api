using FluentValidation;

namespace WorkSpace.Application.Features.Chat.Commands.StartChatThread;

public class StartChatThreadCommandValidator : AbstractValidator<StartChatThreadCommand>
{
    public StartChatThreadCommandValidator()
    {
        RuleFor(c => c.RequestUserId)
            .GreaterThan(0).WithMessage("Người dùng không hợp lệ");
        
        RuleFor(c => c.RequestDto)
            .NotNull().WithMessage("Dữ liệu cuộc trò chuyện không được để trống");
        
        RuleFor(c => c.RequestDto.BookingId)
            .GreaterThan(0).WithMessage("MAĐặt chỗ không hợp lệ")
            .When(c => c.RequestDto is not null);
    }
}