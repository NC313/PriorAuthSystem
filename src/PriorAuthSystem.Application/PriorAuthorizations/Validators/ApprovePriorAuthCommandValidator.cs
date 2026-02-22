using FluentValidation;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.ApprovePriorAuth;

namespace PriorAuthSystem.Application.PriorAuthorizations.Validators;

public class ApprovePriorAuthCommandValidator : AbstractValidator<ApprovePriorAuthCommand>
{
    public ApprovePriorAuthCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEqual(Guid.Empty).WithMessage("Request ID is required.");

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required.");
    }
}
