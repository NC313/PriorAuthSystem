using FluentValidation;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.DenyPriorAuth;
using PriorAuthSystem.Domain.Enums;

namespace PriorAuthSystem.Application.PriorAuthorizations.Validators;

public class DenyPriorAuthCommandValidator : AbstractValidator<DenyPriorAuthCommand>
{
    public DenyPriorAuthCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEqual(Guid.Empty).WithMessage("Request ID is required.");

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required.");

        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("A valid denial reason is required.");

        RuleFor(x => x.Notes)
            .NotEmpty().WithMessage("Denial notes are required.");
    }
}
