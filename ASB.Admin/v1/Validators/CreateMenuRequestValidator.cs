using FluentValidation;
using ASB.Admin.v1.Requests;

namespace ASB.Admin.v1.Validators;

public class CreateMenuRequestValidator : AbstractValidator<CreateMenuRequest>
{
    public CreateMenuRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Menu name is required.")
            .MaximumLength(200).WithMessage("Menu name must not exceed 200 characters.");

        RuleFor(x => x.Route)
            .NotEmpty().WithMessage("Route is required.")
            .MaximumLength(500).WithMessage("Route must not exceed 500 characters.");

        RuleFor(x => x.Icon)
            .MaximumLength(100).WithMessage("Icon must not exceed 100 characters.")
            .When(x => x.Icon is not null);

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be zero or positive.");

        RuleFor(x => x.ParentMenuId)
            .GreaterThan(0).WithMessage("ParentMenuId must be a positive integer.")
            .When(x => x.ParentMenuId.HasValue);
    }
}
