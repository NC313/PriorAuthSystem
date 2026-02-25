using MediatR;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Application.PriorAuthorizations.Commands.SubmitPriorAuth;

public sealed class SubmitPriorAuthCommandHandler : IRequestHandler<SubmitPriorAuthCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitPriorAuthCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(SubmitPriorAuthCommand request, CancellationToken cancellationToken)
    {
        var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId, cancellationToken)
            ?? throw new KeyNotFoundException($"Patient with ID '{request.PatientId}' not found.");

        var provider = await _unitOfWork.Providers.GetByIdAsync(request.ProviderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Provider with ID '{request.ProviderId}' not found.");

        var payer = await _unitOfWork.Payers.GetByIdAsync(request.PayerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Payer with ID '{request.PayerId}' not found.");

        var icdCode = new IcdCode(request.IcdCode, request.IcdDescription);
        var cptCode = new CptCode(request.CptCode, request.CptDescription, request.CptRequiresPriorAuth);
        var clinicalJustification = new ClinicalJustification(
            request.ClinicalNotes,
            request.ClinicalDocumentedBy,
            request.ClinicalSupportingDocumentPath);

        var priorAuth = new PriorAuthorizationRequest(
            patient, provider, payer,
            icdCode, cptCode, clinicalJustification,
            request.RequiredResponseBy);

        priorAuth.Submit();

        await _unitOfWork.PriorAuthorizationRequests.AddAsync(priorAuth, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return priorAuth.Id;
    }
}
