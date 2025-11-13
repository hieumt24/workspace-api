using FluentValidation;

namespace WorkSpace.Application.Features.Chat.Queries.GetChatMessages;

public class GetChatMessagesQueryValidator : AbstractValidator<GetChatMessagesQuery>
{
    public GetChatMessagesQueryValidator()
    {
        RuleFor(q => q.ThreadId)
            .GreaterThan(0).WithMessage("Cuộc trò chuyện không hợp lệ.");

        RuleFor(q => q.RequestUserId)
            .GreaterThan(0).WithMessage("Người dùng không hợp lệ.");
    }
}