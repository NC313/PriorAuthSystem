using Microsoft.AspNetCore.Mvc;
using PriorAuthSystem.Application.PriorAuthorizations.DTOs;
using PriorAuthSystem.Domain.Exceptions;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.API.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var patient = await unitOfWork.Patients.GetByIdAsync(id, cancellationToken);
        if (patient is null)
            return NotFound(new { message = $"Patient with ID '{id}' was not found." });

        return Ok(MapToDto(patient));
    }

    [HttpGet("member/{memberId}")]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByMemberId(string memberId, CancellationToken cancellationToken)
    {
        var patient = await unitOfWork.Patients.GetByMemberIdAsync(memberId, cancellationToken);
        if (patient is null)
            return NotFound(new { message = $"Patient with member ID '{memberId}' was not found." });

        return Ok(MapToDto(patient));
    }

    private static PatientDto MapToDto(Domain.Entities.Patient patient) =>
        new(
            patient.Id,
            patient.FirstName,
            patient.LastName,
            patient.DateOfBirth,
            patient.MemberId,
            patient.InsurancePlanId,
            patient.ContactInfo.Phone,
            patient.ContactInfo.Email,
            patient.ContactInfo.FaxNumber);
}
