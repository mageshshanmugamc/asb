using FluentValidation;
using ASB.Admin.v1.Requests;

namespace ASB.Admin.v1.Validators;

public class ChatRequestValidator : AbstractValidator<ChatRequest>
{
    public ChatRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(5000).WithMessage("Message must not exceed 5000 characters.");

        RuleFor(x => x.ConversationId)
            .MaximumLength(100).WithMessage("ConversationId must not exceed 100 characters.")
            .When(x => x.ConversationId is not null);
    }
}
