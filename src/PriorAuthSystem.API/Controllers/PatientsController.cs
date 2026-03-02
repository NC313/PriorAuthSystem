using Microsoft.AspNetCore.Mvc;
using PriorAuthSystem.Application.PriorAuthorizations.DTOs;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Exceptions;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.API.Controllers;

public sealed record CreatePatientRequest(
    string FirstName, string LastName, string DateOfBirth,
    string MemberId, string InsurancePlanId,
    string Phone, string Email, string FaxNumber);

[ApiController]
[Route("api/patients")]
public class PatientsController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IList<PatientDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var patients = await unitOfWork.Patients.GetAllAsync(cancellationToken);
        return Ok(patients.Select(MapToDto).ToList());
    }

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

    [HttpPost]
    [ProducesResponseType(typeof(PatientDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest req, CancellationToken ct)
    {
        var contact = new ContactInfo(req.Phone, req.Email, req.FaxNumber ?? "");
        var patient = new Patient(req.FirstName, req.LastName,
            DateTime.Parse(req.DateOfBirth), req.MemberId, req.InsurancePlanId ?? "", contact);
        await unitOfWork.Patients.AddAsync(patient, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, MapToDto(patient));
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
