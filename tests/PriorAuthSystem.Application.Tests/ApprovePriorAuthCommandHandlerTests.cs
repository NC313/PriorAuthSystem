using FluentAssertions;
using Moq;
using PriorAuthSystem.Application.Common.Interfaces;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.ApprovePriorAuth;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Enums;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Application.Tests;

public class ApprovePriorAuthCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPriorAuthorizationRepository> _priorAuthRepoMock = new();
    private readonly Mock<IPriorAuthNotificationService> _notificationServiceMock = new();
    private readonly ApprovePriorAuthCommandHandler _handler;

    public ApprovePriorAuthCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.PriorAuthorizationRequests).Returns(_priorAuthRepoMock.Object);

        _handler = new ApprovePriorAuthCommandHandler(
            _unitOfWorkMock.Object,
            _notificationServiceMock.Object);
    }

    private static PriorAuthorizationRequest CreateSubmittedRequest()
    {
        var contact = new ContactInfo("555-0100", "test@example.com");
        var patient = new Patient("John", "Doe", new DateTime(1990, 1, 1), "MEM001", "PLAN001", contact);
        var provider = new Provider("Jane", "Smith", "1234567890", "Cardiology", "Heart Center", contact);
        var payer = new Payer("Acme Insurance", "PAY001", 14, contact);
        var icdCode = new IcdCode("I10", "Essential hypertension");
        var cptCode = new CptCode("99213", "Office visit", true);
        var justification = new ClinicalJustification("Clinical notes", "Dr. Smith");

        var request = new PriorAuthorizationRequest(
            patient, provider, payer,
            icdCode, cptCode, justification,
            DateTime.UtcNow.AddDays(30));

        request.Submit();
        request.ClearDomainEvents();
        return request;
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldApproveAndUpdate()
    {
        var priorAuth = CreateSubmittedRequest();
        var command = new ApprovePriorAuthCommand(priorAuth.Id, "reviewer-1", "Approved.");

        _priorAuthRepoMock.Setup(r => r.GetByIdAsync(priorAuth.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priorAuth);

        await _handler.Handle(command, CancellationToken.None);

        priorAuth.Status.Should().Be(PriorAuthStatus.Approved);
        _priorAuthRepoMock.Verify(
            r => r.UpdateAsync(priorAuth, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSendNotification()
    {
        var priorAuth = CreateSubmittedRequest();
        var command = new ApprovePriorAuthCommand(priorAuth.Id, "reviewer-1", "Approved.");

        _priorAuthRepoMock.Setup(r => r.GetByIdAsync(priorAuth.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(priorAuth);

        await _handler.Handle(command, CancellationToken.None);

        _notificationServiceMock.Verify(
            n => n.SendStatusUpdate(priorAuth.Id, PriorAuthStatus.Approved.ToString()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequestNotFound_ShouldThrowKeyNotFoundException()
    {
        var requestId = Guid.NewGuid();
        var command = new ApprovePriorAuthCommand(requestId, "reviewer-1", "Approved.");

        _priorAuthRepoMock.Setup(r => r.GetByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriorAuthorizationRequest?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{requestId}*");
    }
}
