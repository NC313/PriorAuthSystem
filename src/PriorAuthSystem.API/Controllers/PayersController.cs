using Microsoft.AspNetCore.Mvc;
using PriorAuthSystem.Application.PriorAuthorizations.DTOs;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.API.Controllers;

public sealed record CreatePayerRequest(
    string PayerName, string PayerId, int StandardResponseDays,
    string Phone, string Email, string FaxNumber);

[ApiController]
[Route("api/payers")]
public class PayersController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IList<PayerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var payers = await unitOfWork.Payers.GetAllAsync(cancellationToken);
        return Ok(payers.Select(p => new PayerDto(
            p.Id, p.PayerName, p.PayerId, p.StandardResponseDays,
            p.ContactInfo.Phone, p.ContactInfo.Email, p.ContactInfo.FaxNumber)).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PayerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var payer = await unitOfWork.Payers.GetByIdAsync(id, cancellationToken);
        if (payer is null)
            return NotFound(new { message = $"Payer with ID '{id}' was not found." });

        return Ok(new PayerDto(
            payer.Id, payer.PayerName, payer.PayerId, payer.StandardResponseDays,
            payer.ContactInfo.Phone, payer.ContactInfo.Email, payer.ContactInfo.FaxNumber));
    }

    [HttpPost]
    [ProducesResponseType(typeof(PayerDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreatePayerRequest req, CancellationToken ct)
    {
        var contact = new ContactInfo(req.Phone, req.Email, req.FaxNumber ?? "");
        var payer = new Payer(req.PayerName, req.PayerId, req.StandardResponseDays, contact);
        await unitOfWork.Payers.AddAsync(payer, ct);
        await unitOfWork.SaveChangesAsync(ct);
        var dto = new PayerDto(payer.Id, payer.PayerName, payer.PayerId,
            payer.StandardResponseDays, payer.ContactInfo.Phone,
            payer.ContactInfo.Email, payer.ContactInfo.FaxNumber);
        return CreatedAtAction(nameof(GetById), new { id = payer.Id }, dto);
    }
}
