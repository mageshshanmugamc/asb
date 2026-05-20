using FluentValidation;
using ASB.Admin.v1.Requests;

namespace ASB.Admin.v1.Validators;

public class AssignRoleToUserGroupRequestValidator : AbstractValidator<AssignRoleToUserGroupRequest>
{
    public AssignRoleToUserGroupRequestValidator()
    {
        RuleFor(x => x.UserGroupId)
            .GreaterThan(0).WithMessage("UserGroupId must be a positive integer.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("RoleId must be a positive integer.");
    }
}
