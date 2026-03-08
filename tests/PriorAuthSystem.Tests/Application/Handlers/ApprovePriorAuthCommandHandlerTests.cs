using FluentAssertions;
using Moq;
using PriorAuthSystem.Application.Common.Interfaces;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.ApprovePriorAuth;
using PriorAuthSystem.Domain.Enums;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Tests.Helpers;

namespace PriorAuthSystem.Tests.Application.Handlers;

public class ApprovePriorAuthCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPriorAuthorizationRepository> _repo = new();
    private readonly Mock<IPriorAuthNotificationService> _notifications = new();
    private readonly ApprovePriorAuthCommandHandler _handler;

    public ApprovePriorAuthCommandHandlerTests()
    {
        _unitOfWork.Setup(u => u.PriorAuthorizationRequests).Returns(_repo.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _notifications.Setup(n => n.SendStatusUpdate(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _handler = new ApprovePriorAuthCommandHandler(_unitOfWork.Object, _notifications.Object);
    }

    [Fact]
    public async Task Handle_WithValidSubmittedRequest_ApprovesAndSaves()
    {
        var request = PriorAuthFactory.CreateSubmitted();
        _repo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        var command = new ApprovePriorAuthCommand(request.Id, "reviewer-1", "Clinically appropriate.");

        await _handler.Handle(command, CancellationToken.None);

        request.Status.Should().Be(PriorAuthStatus.Approved);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_AfterApproval_SendsSignalRNotification()
    {
        var request = PriorAuthFactory.CreateSubmitted();
        _repo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        var command = new ApprovePriorAuthCommand(request.Id, "reviewer-1", "ok");

        await _handler.Handle(command, CancellationToken.None);

        _notifications.Verify(n => n.SendStatusUpdate(request.Id, "Approved"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequestNotFound_ThrowsKeyNotFoundException()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PriorAuthorizationRequest?)null);
        var command = new ApprovePriorAuthCommand(Guid.NewGuid(), "reviewer-1", "notes");

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RecordsReviewerNotes()
    {
        var request = PriorAuthFactory.CreateSubmitted();
        _repo.Setup(r => r.GetByIdAsync(request.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        var command = new ApprovePriorAuthCommand(request.Id, "reviewer-1", "Meets criteria.");

        await _handler.Handle(command, CancellationToken.None);

        request.StatusTransitions.Last().Notes.Should().Be("Meets criteria.");
    }
}
