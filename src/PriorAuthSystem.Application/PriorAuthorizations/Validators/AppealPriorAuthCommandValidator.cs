using FluentValidation;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.AppealPriorAuth;

namespace PriorAuthSystem.Application.PriorAuthorizations.Validators;

public class AppealPriorAuthCommandValidator : AbstractValidator<AppealPriorAuthCommand>
{
    public AppealPriorAuthCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEqual(Guid.Empty).WithMessage("Request ID is required.");

        RuleFor(x => x.AppealedBy)
            .NotEmpty().WithMessage("Appealed by is required.");

        RuleFor(x => x.ClinicalJustification)
            .NotEmpty().WithMessage("Clinical justification is required.");
    }
}
