using FluentValidation;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.RequestAdditionalInfo;

namespace PriorAuthSystem.Application.PriorAuthorizations.Validators;

public class RequestAdditionalInfoCommandValidator : AbstractValidator<RequestAdditionalInfoCommand>
{
    public RequestAdditionalInfoCommandValidator()
    {
        RuleFor(x => x.RequestId)
            .NotEqual(Guid.Empty).WithMessage("Request ID is required.");

        RuleFor(x => x.RequestedBy)
            .NotEmpty().WithMessage("Requested by is required.");

        RuleFor(x => x.Notes)
            .NotEmpty().WithMessage("Notes are required.");
    }
}
