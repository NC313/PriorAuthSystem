using FluentAssertions;
using Moq;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.SubmitPriorAuth;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Enums;
using PriorAuthSystem.Domain.Interfaces;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Application.Tests;

public class SubmitPriorAuthCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPriorAuthorizationRepository> _priorAuthRepoMock = new();
    private readonly Mock<IPatientRepository> _patientRepoMock = new();
    private readonly Mock<IProviderRepository> _providerRepoMock = new();
    private readonly Mock<IPayerRepository> _payerRepoMock = new();
    private readonly SubmitPriorAuthCommandHandler _handler;

    private readonly ContactInfo _contact = new("555-0100", "test@example.com");

    public SubmitPriorAuthCommandHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.PriorAuthorizationRequests).Returns(_priorAuthRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Patients).Returns(_patientRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Providers).Returns(_providerRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Payers).Returns(_payerRepoMock.Object);

        _handler = new SubmitPriorAuthCommandHandler(_unitOfWorkMock.Object);
    }

    private SubmitPriorAuthCommand CreateValidCommand(
        Guid? patientId = null,
        Guid? providerId = null,
        Guid? payerId = null) => new(
            PatientId: patientId ?? Guid.NewGuid(),
            ProviderId: providerId ?? Guid.NewGuid(),
            PayerId: payerId ?? Guid.NewGuid(),
            IcdCode: "I10",
            IcdDescription: "Essential hypertension",
            CptCode: "99213",
            CptDescription: "Office visit",
            CptRequiresPriorAuth: true,
            ClinicalNotes: "Patient requires treatment",
            ClinicalDocumentedBy: "Dr. Smith",
            ClinicalSupportingDocumentPath: "/docs/evidence.pdf",
            RequiredResponseBy: DateTime.UtcNow.AddDays(14));

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnNewId()
    {
        var patient = new Patient("John", "Doe", new DateTime(1990, 1, 1), "MEM001", "PLAN001", _contact);
        var provider = new Provider("Jane", "Smith", "1234567890", "Cardiology", "Heart Center", _contact);
        var payer = new Payer("Acme Insurance", "PAY001", 14, _contact);

        var command = CreateValidCommand(patient.Id, provider.Id, payer.Id);

        _patientRepoMock.Setup(r => r.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _providerRepoMock.Setup(r => r.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);
        _payerRepoMock.Setup(r => r.GetByIdAsync(payer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payer);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCallAddAndSave()
    {
        var patient = new Patient("John", "Doe", new DateTime(1990, 1, 1), "MEM001", "PLAN001", _contact);
        var provider = new Provider("Jane", "Smith", "1234567890", "Cardiology", "Heart Center", _contact);
        var payer = new Payer("Acme Insurance", "PAY001", 14, _contact);

        var command = CreateValidCommand(patient.Id, provider.Id, payer.Id);

        _patientRepoMock.Setup(r => r.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _providerRepoMock.Setup(r => r.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);
        _payerRepoMock.Setup(r => r.GetByIdAsync(payer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payer);

        await _handler.Handle(command, CancellationToken.None);

        _priorAuthRepoMock.Verify(
            r => r.AddAsync(It.Is<PriorAuthorizationRequest>(pa => pa.Status == PriorAuthStatus.Submitted),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ShouldThrowKeyNotFoundException()
    {
        var command = CreateValidCommand();

        _patientRepoMock.Setup(r => r.GetByIdAsync(command.PatientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Patient?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{command.PatientId}*");
    }

    [Fact]
    public async Task Handle_WhenProviderNotFound_ShouldThrowKeyNotFoundException()
    {
        var patient = new Patient("John", "Doe", new DateTime(1990, 1, 1), "MEM001", "PLAN001", _contact);
        var command = CreateValidCommand(patientId: patient.Id);

        _patientRepoMock.Setup(r => r.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _providerRepoMock.Setup(r => r.GetByIdAsync(command.ProviderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{command.ProviderId}*");
    }

    [Fact]
    public async Task Handle_WhenPayerNotFound_ShouldThrowKeyNotFoundException()
    {
        var patient = new Patient("John", "Doe", new DateTime(1990, 1, 1), "MEM001", "PLAN001", _contact);
        var provider = new Provider("Jane", "Smith", "1234567890", "Cardiology", "Heart Center", _contact);
        var command = CreateValidCommand(patientId: patient.Id, providerId: provider.Id);

        _patientRepoMock.Setup(r => r.GetByIdAsync(patient.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _providerRepoMock.Setup(r => r.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);
        _payerRepoMock.Setup(r => r.GetByIdAsync(command.PayerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payer?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{command.PayerId}*");
    }
}
