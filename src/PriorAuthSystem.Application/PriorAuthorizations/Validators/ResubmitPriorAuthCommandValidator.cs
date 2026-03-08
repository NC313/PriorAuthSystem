using FluentValidation;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.ResubmitPriorAuth;

namespace PriorAuthSystem.Application.PriorAuthorizations.Validators;

public class ResubmitPriorAuthCommandValidator : AbstractValidator<ResubmitPriorAuthCommand>
{
    public ResubmitPriorAuthCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEqual(Guid.Empty).WithMessage("Request ID is required.");

        RuleFor(x => x.ResubmittedBy)
            .NotEmpty().WithMessage("Resubmitted by is required.");
    }
}
