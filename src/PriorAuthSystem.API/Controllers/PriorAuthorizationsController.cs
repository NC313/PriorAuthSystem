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
using PriorAuthSystem.Domain.Enums;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Infrastructure.Services;

namespace PriorAuthSystem.API.Controllers;

[ApiController]
[Route("api/prior-authorizations")]
public class PriorAuthorizationsController(ISender mediator, IUnitOfWork unitOfWork, AuditService auditService) : ControllerBase
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

    [HttpGet("all")]
    [ProducesResponseType(typeof(IList<PriorAuthSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var all = await unitOfWork.PriorAuthorizationRequests.GetAllAsync(cancellationToken);
        var summaries = all.Select(pa => new PriorAuthSummaryDto(
            pa.Id, pa.Patient.FullName, pa.Provider.FullName, pa.Payer.PayerName,
            pa.CptCode.Code, pa.IcdCode.Code, pa.Status.ToString(),
            pa.CreatedAt, pa.RequiredResponseBy)).ToList();
        return Ok(summaries);
    }

    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var all = await unitOfWork.PriorAuthorizationRequests.GetAllAsync(cancellationToken);

        var pending = all.Count(a => a.Status is PriorAuthStatus.Submitted or PriorAuthStatus.UnderReview);
        var approved = all.Count(a => a.Status is PriorAuthStatus.Approved or PriorAuthStatus.AppealApproved);
        var denied = all.Count(a => a.Status is PriorAuthStatus.Denied or PriorAuthStatus.AppealDenied);
        var underReview = all.Count(a => a.Status == PriorAuthStatus.UnderReview);

        var completedAuths = all.Where(a => a.Status is PriorAuthStatus.Approved or PriorAuthStatus.Denied
            or PriorAuthStatus.AppealApproved or PriorAuthStatus.AppealDenied).ToList();
        var avgResponseDays = completedAuths.Count > 0
            ? completedAuths.Average(a => (a.UpdatedAt ?? a.CreatedAt).Subtract(a.CreatedAt).TotalDays)
            : 0.0;

        var denialReasonBreakdown = all
            .SelectMany(a => a.StatusTransitions)
            .Where(t => t.ToStatus == PriorAuthStatus.Denied && !string.IsNullOrEmpty(t.Notes))
            .GroupBy(t =>
            {
                var notes = t.Notes;
                if (notes.StartsWith('[') && notes.Contains(']'))
                    return notes[1..notes.IndexOf(']')];
                return "Other";
            })
            .ToDictionary(g => g.Key, g => g.Count());

        return Ok(new
        {
            pending,
            approved,
            denied,
            underReview,
            avgResponseDays = Math.Round(avgResponseDays, 1),
            denialReasonBreakdown
        });
    }

    [HttpGet("audit-log")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAuditLog()
    {
        var entries = auditService.GetRecentEntries(50);
        return Ok(entries);
    }
}

public sealed record ApproveRequest(string ReviewerId, string Notes);
public sealed record DenyRequest(string ReviewerId, Domain.Enums.DenialReason Reason, string Notes);
public sealed record AdditionalInfoRequest(string RequestedBy, string Notes);
public sealed record AppealRequest(string AppealedBy, string ClinicalJustification);
