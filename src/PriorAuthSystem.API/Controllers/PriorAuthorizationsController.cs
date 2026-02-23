using MediatR;
using Microsoft.AspNetCore.Mvc;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.AppealPriorAuth;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.ApprovePriorAuth;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.DenyPriorAuth;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.RequestAdditionalInfo;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.SubmitPriorAuth;
using PriorAuthSystem.Application.PriorAuthorizations.DTOs;
using PriorAuthSystem.Application.PriorAuthorizations.Queries.GetPendingPriorAuths;
using PriorAuthSystem.Application.PriorAuthorizations.Queries.GetPriorAuthById;
using PriorAuthSystem.Application.PriorAuthorizations.Queries.GetPriorAuthsByPatient;

namespace PriorAuthSystem.API.Controllers;

[ApiController]
[Route("api/prior-authorizations")]
public class PriorAuthorizationsController(ISender mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitPriorAuthCommand command,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PriorAuthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPriorAuthByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(IList<PriorAuthSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatient(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPriorAuthsByPatientQuery(patientId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("pending")]
    [ProducesResponseType(typeof(IList<PriorAuthSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPendingPriorAuthsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        Guid id,
        [FromBody] ApproveRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new ApprovePriorAuthCommand(id, request.ReviewerId, request.Notes), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/deny")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deny(
        Guid id,
        [FromBody] DenyRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DenyPriorAuthCommand(id, request.ReviewerId, request.Reason, request.Notes), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/additional-info")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RequestAdditionalInfo(
        Guid id,
        [FromBody] AdditionalInfoRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new RequestAdditionalInfoCommand(id, request.RequestedBy, request.Notes), cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/appeal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Appeal(
        Guid id,
        [FromBody] AppealRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new AppealPriorAuthCommand(id, request.AppealedBy, request.ClinicalJustification), cancellationToken);
        return NoContent();
    }
}

public sealed record ApproveRequest(string ReviewerId, string Notes);
public sealed record DenyRequest(string ReviewerId, Domain.Enums.DenialReason Reason, string Notes);
public sealed record AdditionalInfoRequest(string RequestedBy, string Notes);
public sealed record AppealRequest(string AppealedBy, string ClinicalJustification);
