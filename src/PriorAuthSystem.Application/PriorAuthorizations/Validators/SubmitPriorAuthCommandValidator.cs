using FluentValidation;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.SubmitPriorAuth;

namespace PriorAuthSystem.Application.PriorAuthorizations.Validators;

public class SubmitPriorAuthCommandValidator : AbstractValidator<SubmitPriorAuthCommand>
{
    public SubmitPriorAuthCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEqual(Guid.Empty).WithMessage("Patient ID is required.");

        RuleFor(x => x.ProviderId)
            .NotEqual(Guid.Empty).WithMessage("Provider ID is required.");

        RuleFor(x => x.PayerId)
            .NotEqual(Guid.Empty).WithMessage("Payer ID is required.");

        RuleFor(x => x.IcdCode)
            .NotEmpty().WithMessage("ICD-10 code is required.")
            .MinimumLength(3).WithMessage("ICD-10 code must be at least 3 characters.")
            .MaximumLength(7).WithMessage("ICD-10 code must be at most 7 characters.");

        RuleFor(x => x.CptCode)
            .NotEmpty().WithMessage("CPT code is required.")
            .Length(5).WithMessage("CPT code must be exactly 5 characters.");

        RuleFor(x => x.ClinicalNotes)
            .NotEmpty().WithMessage("Clinical justification notes are required.");

        RuleFor(x => x.ClinicalDocumentedBy)
            .NotEmpty().WithMessage("Documenting provider is required.");

        RuleFor(x => x.RequiredResponseBy)
            .GreaterThan(DateTime.UtcNow).WithMessage("Required response date must be in the future.");
    }
}
