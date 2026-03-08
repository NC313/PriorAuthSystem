using FluentAssertions;
using Moq;
using PriorAuthSystem.Application.Common.Interfaces;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.DenyPriorAuth;
using PriorAuthSystem.Domain.Enums;
using PriorAuthSystem.Domain.Exceptions;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Tests.Helpers;

namespace PriorAuthSystem.Tests.Application.Handlers;

public class DenyPriorAuthCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPriorAuthorizationRepository> _repo = new();
    private readonly Mock<IPriorAuthNotificationService> _notifications = new();
    private readonly DenyPriorAuthCommandHandler _handler;

    public DenyPriorAuthCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.PriorAuthorizationRequests).Returns(_repo.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _notifications.Setup(n => n.SendStatusUpdate(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _handler = new DenyPriorAuthCommandHandler(_unitOfWork.Object, _notifications.Object);
    }

    [Fact]
    public async Task Handle_WithValidSubmittedRequest_DeniesAndSaves()
    {
        var request = PriorAuthFactory.CreateSubmitted();
        _repo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        var command = new DenyPriorAuthCommand(request.Id, "reviewer-1", DenialReason.NotMedicallyNecessary, "Not supported by evidence.");

        await _handler.Handle(command, CancellationToken.None);

        request.Status.Should().Be(PriorAuthStatus.Denied);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AfterDenial_SendsSignalRNotification()
    {
        var request = PriorAuthFactory.CreateSubmitted();
        _repo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        var command = new DenyPriorAuthCommand(request.Id, "reviewer-1", DenialReason.OutOfNetwork, "");

        await _handler.Handle(command, CancellationToken.None);

        _notifications.Verify(n => n.SendStatusUpdate(request.Id, "Denied"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequestNotFound_ThrowsKeyNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriorAuthorizationRequest?)null);
        var command = new DenyPriorAuthCommand(Guid.NewGuid(), "reviewer-1", DenialReason.Other, "notes");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmbedsDenialReasonInNotes()
    {
        var request = PriorAuthFactory.CreateSubmitted();
        _repo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        var command = new DenyPriorAuthCommand(request.Id, "reviewer-1", DenialReason.ServiceNotCovered, "Plan exclusion.");

        await _handler.Handle(command, CancellationToken.None);

        request.StatusTransitions.Last().Notes.Should().Contain("ServiceNotCovered");
    }

    [Fact]
    public async Task Handle_DenyingAlreadyApprovedRequest_ThrowsInvalidStatusTransitionException()
    {
        var request = PriorAuthFactory.CreateSubmitted();
        request.Approve("reviewer-1", "Already approved.");
        _repo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        var command = new DenyPriorAuthCommand(request.Id, "reviewer-2", DenialReason.Other, "notes");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidStatusTransitionException>();
    }
}
