using FluentValidation;
using ASB.Admin.v1.Requests;

namespace ASB.Admin.v1.Validators;

public class IngestPdfRequestValidator : AbstractValidator<IngestPdfRequest>
{
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB
    private static readonly string[] AllowedMimeTypes = { "application/pdf" };

    public IngestPdfRequestValidator()
    {
        RuleFor(x => x.PdfFile)
            .NotNull().WithMessage("PDF file is required.")
            .Must(file => file!.Length > 0).WithMessage("PDF file cannot be empty.")
            .Must(file => file!.Length <= MaxFileSizeBytes)
            .WithMessage($"PDF file must not exceed {MaxFileSizeBytes / (1024 * 1024)} MB.");

        RuleFor(x => x.PdfFile!)
            .Must(file => AllowedMimeTypes.Contains(file.ContentType))
            .WithMessage("Only PDF files are allowed.")
            .When(x => x.PdfFile != null);

        RuleFor(x => x.Source)
            .MaximumLength(500).WithMessage("Source must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Source));

        RuleFor(x => x.ChunkSize)
            .GreaterThan(0).WithMessage("ChunkSize must be greater than zero.")
            .LessThanOrEqualTo(10000).WithMessage("ChunkSize must not exceed 10000.");

        RuleFor(x => x.ChunkOverlap)
            .GreaterThanOrEqualTo(0).WithMessage("ChunkOverlap must be zero or positive.")
            .LessThan(x => x.ChunkSize).WithMessage("ChunkOverlap must be less than ChunkSize.");
    }
}
