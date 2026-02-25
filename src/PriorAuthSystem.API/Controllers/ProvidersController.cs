using Microsoft.AspNetCore.Mvc;
using PriorAuthSystem.Application.PriorAuthorizations.DTOs;
using PriorAuthSystem.Domain.Interfaces;

namespace PriorAuthSystem.API.Controllers;

[ApiController]
[Route("api/providers")]
public class ProvidersController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IList<ProviderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var providers = await unitOfWork.Providers.GetAllAsync(cancellationToken);
        return Ok(providers.Select(p => new ProviderDto(
            p.Id, p.FirstName, p.LastName, p.NPI, p.Specialty, p.OrganizationName,
            p.ContactInfo.Phone, p.ContactInfo.Email, p.ContactInfo.FaxNumber)).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProviderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var provider = await unitOfWork.Providers.GetByIdAsync(id, cancellationToken);
        if (provider is null)
            return NotFound(new { message = $"Provider with ID '{id}' was not found." });

        return Ok(new ProviderDto(
            provider.Id, provider.FirstName, provider.LastName, provider.NPI,
            provider.Specialty, provider.OrganizationName,
            provider.ContactInfo.Phone, provider.ContactInfo.Email, provider.ContactInfo.FaxNumber));
    }
}
