using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Infrastructure.Services;

namespace PriorAuthSystem.API.Controllers;

[ApiController]
[Route("api/fhir")]
public class FhirController(IUnitOfWork unitOfWork, FhirMappingService fhirMappingService) : ControllerBase
{
    private static readonly FhirJsonSerializer Serializer = new();

    [HttpGet("Patient/{id:guid}")]
    [Produces("application/fhir+json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFhirPatient(Guid id, CancellationToken cancellationToken)
    {
        var patient = await unitOfWork.Patients.GetByIdAsync(id, cancellationToken);
        if (patient is null)
            return NotFound(new { message = $"Patient with ID '{id}' was not found." });

        var fhirPatient = fhirMappingService.MapToFhirPatient(patient);
        var json = Serializer.SerializeToString(fhirPatient);
        return Content(json, "application/fhir+json");
    }

    [HttpGet("Claim/{id:guid}")]
    [Produces("application/fhir+json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFhirClaim(Guid id, CancellationToken cancellationToken)
    {
        var priorAuth = await unitOfWork.PriorAuthorizationRequests.GetByIdAsync(id, cancellationToken);
        if (priorAuth is null)
            return NotFound(new { message = $"Prior authorization request with ID '{id}' was not found." });

        var fhirClaim = fhirMappingService.MapToFhirClaim(priorAuth);
        var json = Serializer.SerializeToString(fhirClaim);
        return Content(json, "application/fhir+json");
    }
}
