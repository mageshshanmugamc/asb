using FluentValidation;
using ASB.Admin.v1.Requests;

namespace ASB.Admin.v1.Validators;

public class IngestDocumentRequestValidator : AbstractValidator<IngestDocumentRequest>
{
    public IngestDocumentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.");

        RuleFor(x => x.Source)
            .NotEmpty().WithMessage("Source is required.")
            .MaximumLength(500).WithMessage("Source must not exceed 500 characters.");

        RuleFor(x => x.ChunkSize)
            .GreaterThan(0).WithMessage("ChunkSize must be greater than zero.")
            .LessThanOrEqualTo(10000).WithMessage("ChunkSize must not exceed 10000.");

        RuleFor(x => x.ChunkOverlap)
            .GreaterThanOrEqualTo(0).WithMessage("ChunkOverlap must be zero or positive.")
            .LessThan(x => x.ChunkSize).WithMessage("ChunkOverlap must be less than ChunkSize.");
    }
}
