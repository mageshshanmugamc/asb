using FluentValidation;
using ASB.Admin.v1.Requests;

namespace ASB.Admin.v1.Validators;

public class CreateUserGroupRequestValidator : AbstractValidator<CreateUserGroupRequest>
{
    public CreateUserGroupRequestValidator()
    {
        RuleFor(x => x.GroupName)
            .NotEmpty().WithMessage("Group name is required.")
            .MaximumLength(200).WithMessage("Group name must not exceed 200 characters.");

        RuleForEach(x => x.RoleIds)
            .GreaterThan(0).WithMessage("Each RoleId must be a positive integer.");
    }
}
