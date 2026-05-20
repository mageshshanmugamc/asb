using FluentValidation;
using ASB.Admin.v1.Requests;

namespace ASB.Admin.v1.Validators;

public class UpdatePolicyRequestValidator : AbstractValidator<UpdatePolicyRequest>
{
    public UpdatePolicyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Policy name is required.")
            .MaximumLength(200).WithMessage("Policy name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.Resource)
            .NotEmpty().WithMessage("Resource is required.")
            .MaximumLength(200).WithMessage("Resource must not exceed 200 characters.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .MaximumLength(200).WithMessage("Action must not exceed 200 characters.");
    }
}
