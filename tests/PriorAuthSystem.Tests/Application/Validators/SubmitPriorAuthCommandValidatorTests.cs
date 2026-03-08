using FluentAssertions;
using PriorAuthSystem.Application.PriorAuthorizations.Commands.SubmitPriorAuth;
using PriorAuthSystem.Application.PriorAuthorizations.Validators;

namespace PriorAuthSystem.Tests.Application.Validators;

public class SubmitPriorAuthCommandValidatorTests
{
    private readonly SubmitPriorAuthCommandValidator _validator = new();

    private static SubmitPriorAuthCommand ValidCommand() => new(
        PatientId: Guid.NewGuid(),
        ProviderId: Guid.NewGuid(),
        PayerId: Guid.NewGuid(),
        IcdCode: "Z51.11",
        IcdDescription: "Chemotherapy encounter",
        CptCode: "96413",
        CptDescription: "Chemotherapy infusion",
        CptRequiresPriorAuth: true,
        ClinicalNotes: "Patient requires chemotherapy following diagnosis.",
        ClinicalDocumentedBy: "Dr. Smith",
        ClinicalSupportingDocumentPath: "",
        RequiredResponseBy: DateTime.UtcNow.AddDays(14));

    [Fact]
    public async Task ValidateAsync_WithValidCommand_PassesValidation()
    {
        var result = await _validator.ValidateAsync(ValidCommand());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyPatientId_FailsValidation()
    {
        var command = ValidCommand() with { PatientId = Guid.Empty };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.PatientId));
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyProviderId_FailsValidation()
    {
        var command = ValidCommand() with { ProviderId = Guid.Empty };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ProviderId));
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyPayerId_FailsValidation()
    {
        var command = ValidCommand() with { PayerId = Guid.Empty };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.PayerId));
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyIcdCode_FailsValidation()
    {
        var command = ValidCommand() with { IcdCode = "" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.IcdCode));
    }

    [Fact]
    public async Task ValidateAsync_WithIcdCodeTooShort_FailsValidation()
    {
        var command = ValidCommand() with { IcdCode = "AB" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.IcdCode));
    }

    [Fact]
    public async Task ValidateAsync_WithIcdCodeTooLong_FailsValidation()
    {
        var command = ValidCommand() with { IcdCode = "ABCDEFGH" }; // 8 chars

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.IcdCode));
    }

    [Fact]
    public async Task ValidateAsync_WithCptCodeNotFiveChars_FailsValidation()
    {
        var command = ValidCommand() with { CptCode = "9641" }; // 4 chars

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CptCode));
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyClinicalNotes_FailsValidation()
    {
        var command = ValidCommand() with { ClinicalNotes = "" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ClinicalNotes));
    }

    [Fact]
    public async Task ValidateAsync_WithEmptyDocumentedBy_FailsValidation()
    {
        var command = ValidCommand() with { ClinicalDocumentedBy = "" };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ClinicalDocumentedBy));
    }

    [Fact]
    public async Task ValidateAsync_WithResponseDateInPast_FailsValidation()
    {
        var command = ValidCommand() with { RequiredResponseBy = DateTime.UtcNow.AddDays(-1) };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.RequiredResponseBy));
    }
}
