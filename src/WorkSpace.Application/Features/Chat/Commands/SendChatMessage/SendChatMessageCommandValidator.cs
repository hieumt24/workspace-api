using FluentValidation;

namespace WorkSpace.Application.Features.Chat.Commands.SendChatMessage;

public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    public SendChatMessageCommandValidator()
    {
        RuleFor(c => c.SenderId)
            .GreaterThan(0).WithMessage("Người gửi không hợp lệ");
        
        RuleFor(c => c.RequestDto)
            .NotNull().WithMessage("Dữ liệu tin nhắn không được để trống");
        
        RuleFor(c => c.RequestDto.ThreadId)
            .GreaterThan(0).WithMessage("Cuộc trò chuyện không hợp lệ")
            .When(c => c.RequestDto is not null);
        
        RuleFor(c => c.RequestDto.Content)
            .NotEmpty().WithMessage("Nội dung tin nhắn không được để trống")
            .Must(content => !string.IsNullOrWhiteSpace(content))
            .WithMessage("Nội dung tin nhắn không được để trống")
            .MaximumLength(5000).WithMessage("Tin nhắn không được vượt quá 5000 ký tự")
            .When(c => c.RequestDto is not null);
    }
}