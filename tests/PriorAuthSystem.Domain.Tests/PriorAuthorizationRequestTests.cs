using System.Reflection;
using FluentAssertions;
using PriorAuthSystem.Domain.Common;
using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.Enums;
using PriorAuthSystem.Domain.Events;
using PriorAuthSystem.Domain.Exceptions;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Domain.Tests;

public class PriorAuthorizationRequestTests
{
    private static PriorAuthorizationRequest CreateDraftRequest()
    {
        var contact = new ContactInfo("555-0100", "test@example.com");
        var patient = new Patient("John", "Doe", new DateTime(1990, 1, 1), "MEM001", "PLAN001", contact);
        var provider = new Provider("Jane", "Smith", "1234567890", "Cardiology", "Heart Center", contact);
        var payer = new Payer("Acme Insurance", "PAY001", 14, contact);
        var icdCode = new IcdCode("I10", "Essential hypertension");
        var cptCode = new CptCode("99213", "Office visit", true);
        var justification = new ClinicalJustification("Clinical notes", "Dr. Smith");

        return new PriorAuthorizationRequest(
            patient, provider, payer,
            icdCode, cptCode, justification,
            DateTime.UtcNow.AddDays(30));
    }

    private static void SetStatus(PriorAuthorizationRequest request, PriorAuthStatus status)
    {
        typeof(PriorAuthorizationRequest)
            .GetProperty(nameof(PriorAuthorizationRequest.Status))!
            .SetValue(request, status);
    }

    private static void SetRequiredResponseBy(PriorAuthorizationRequest request, DateTime date)
    {
        typeof(PriorAuthorizationRequest)
            .GetProperty(nameof(PriorAuthorizationRequest.RequiredResponseBy))!
            .SetValue(request, date);
    }

    // --- Valid Transitions ---

    [Fact]
    public void Constructor_ShouldCreateWithDraftStatus()
    {
        var request = CreateDraftRequest();

        request.Status.Should().Be(PriorAuthStatus.Draft);
        request.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Submit_FromDraft_ShouldTransitionToSubmitted()
    {
        var request = CreateDraftRequest();

        request.Submit();

        request.Status.Should().Be(PriorAuthStatus.Submitted);
    }

    [Fact]
    public void Submit_FromAdditionalInfoRequested_ShouldTransitionToSubmitted()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.AdditionalInfoRequested);

        request.Submit();

        request.Status.Should().Be(PriorAuthStatus.Submitted);
    }

    [Fact]
    public void Approve_FromUnderReview_ShouldTransitionToApproved()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.UnderReview);

        request.Approve("reviewer-1", "Approved after review.");

        request.Status.Should().Be(PriorAuthStatus.Approved);
    }

    [Fact]
    public void Deny_FromUnderReview_ShouldTransitionToDenied()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.UnderReview);

        request.Deny("reviewer-1", DenialReason.NotMedicallyNecessary, "Not justified.");

        request.Status.Should().Be(PriorAuthStatus.Denied);
    }

    [Fact]
    public void RequestAdditionalInfo_FromUnderReview_ShouldTransitionToAdditionalInfoRequested()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.UnderReview);

        request.RequestAdditionalInfo("reviewer-1", "Need more docs.");

        request.Status.Should().Be(PriorAuthStatus.AdditionalInfoRequested);
    }

    [Fact]
    public void ExpireIfOverdue_FromSubmitted_ShouldTransitionToExpired()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.Submitted);
        SetRequiredResponseBy(request, DateTime.UtcNow.AddDays(-1));

        request.ExpireIfOverdue();

        request.Status.Should().Be(PriorAuthStatus.Expired);
    }

    [Fact]
    public void Appeal_FromDenied_ShouldTransitionToAppealed()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.Denied);

        request.Appeal("provider-1", "New evidence supports authorization.");

        request.Status.Should().Be(PriorAuthStatus.Appealed);
    }

    [Fact]
    public void AppealApprove_FromAppealed_ShouldTransitionToAppealApproved()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.Appealed);

        request.AppealApprove("reviewer-2", "Appeal granted.");

        request.Status.Should().Be(PriorAuthStatus.AppealApproved);
    }

    [Fact]
    public void AppealDeny_FromAppealed_ShouldTransitionToAppealDenied()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.Appealed);

        request.AppealDeny("reviewer-2", "Appeal denied — insufficient evidence.");

        request.Status.Should().Be(PriorAuthStatus.AppealDenied);
    }

    // --- Invalid Transitions ---

    [Theory]
    [InlineData(PriorAuthStatus.Approved)]
    [InlineData(PriorAuthStatus.Denied)]
    [InlineData(PriorAuthStatus.Expired)]
    [InlineData(PriorAuthStatus.UnderReview)]
    [InlineData(PriorAuthStatus.Appealed)]
    [InlineData(PriorAuthStatus.AppealApproved)]
    [InlineData(PriorAuthStatus.AppealDenied)]
    public void Submit_FromInvalidStatus_ShouldThrow(PriorAuthStatus invalidFrom)
    {
        var request = CreateDraftRequest();
        SetStatus(request, invalidFrom);

        var act = () => request.Submit();

        act.Should().Throw<InvalidStatusTransitionException>()
            .Where(ex => ex.FromStatus == invalidFrom && ex.ToStatus == PriorAuthStatus.Submitted);
    }

    [Theory]
    [InlineData(PriorAuthStatus.Draft)]
    [InlineData(PriorAuthStatus.Approved)]
    [InlineData(PriorAuthStatus.Denied)]
    [InlineData(PriorAuthStatus.Expired)]
    [InlineData(PriorAuthStatus.AdditionalInfoRequested)]
    [InlineData(PriorAuthStatus.Appealed)]
    public void Approve_FromInvalidStatus_ShouldThrow(PriorAuthStatus invalidFrom)
    {
        var request = CreateDraftRequest();
        SetStatus(request, invalidFrom);

        var act = () => request.Approve("reviewer", "notes");

        act.Should().Throw<InvalidStatusTransitionException>()
            .Where(ex => ex.FromStatus == invalidFrom && ex.ToStatus == PriorAuthStatus.Approved);
    }

    [Theory]
    [InlineData(PriorAuthStatus.Draft)]
    [InlineData(PriorAuthStatus.Approved)]
    [InlineData(PriorAuthStatus.Expired)]
    [InlineData(PriorAuthStatus.AdditionalInfoRequested)]
    [InlineData(PriorAuthStatus.Appealed)]
    public void Deny_FromInvalidStatus_ShouldThrow(PriorAuthStatus invalidFrom)
    {
        var request = CreateDraftRequest();
        SetStatus(request, invalidFrom);

        var act = () => request.Deny("reviewer", DenialReason.Other, "reason");

        act.Should().Throw<InvalidStatusTransitionException>()
            .Where(ex => ex.FromStatus == invalidFrom && ex.ToStatus == PriorAuthStatus.Denied);
    }

    [Theory]
    [InlineData(PriorAuthStatus.Draft)]
    [InlineData(PriorAuthStatus.Approved)]
    [InlineData(PriorAuthStatus.Expired)]
    [InlineData(PriorAuthStatus.Appealed)]
    public void RequestAdditionalInfo_FromInvalidStatus_ShouldThrow(PriorAuthStatus invalidFrom)
    {
        var request = CreateDraftRequest();
        SetStatus(request, invalidFrom);

        var act = () => request.RequestAdditionalInfo("reviewer", "needs info");

        act.Should().Throw<InvalidStatusTransitionException>()
            .Where(ex => ex.FromStatus == invalidFrom && ex.ToStatus == PriorAuthStatus.AdditionalInfoRequested);
    }

    [Theory]
    [InlineData(PriorAuthStatus.Draft)]
    [InlineData(PriorAuthStatus.Submitted)]
    [InlineData(PriorAuthStatus.Approved)]
    [InlineData(PriorAuthStatus.Expired)]
    [InlineData(PriorAuthStatus.AppealApproved)]
    public void Appeal_FromInvalidStatus_ShouldThrow(PriorAuthStatus invalidFrom)
    {
        var request = CreateDraftRequest();
        SetStatus(request, invalidFrom);

        var act = () => request.Appeal("provider", "justification");

        act.Should().Throw<InvalidStatusTransitionException>()
            .Where(ex => ex.FromStatus == invalidFrom && ex.ToStatus == PriorAuthStatus.Appealed);
    }

    [Theory]
    [InlineData(PriorAuthStatus.Draft)]
    [InlineData(PriorAuthStatus.Submitted)]
    [InlineData(PriorAuthStatus.Approved)]
    [InlineData(PriorAuthStatus.Denied)]
    public void AppealApprove_FromInvalidStatus_ShouldThrow(PriorAuthStatus invalidFrom)
    {
        var request = CreateDraftRequest();
        SetStatus(request, invalidFrom);

        var act = () => request.AppealApprove("reviewer", "notes");

        act.Should().Throw<InvalidStatusTransitionException>()
            .Where(ex => ex.FromStatus == invalidFrom && ex.ToStatus == PriorAuthStatus.AppealApproved);
    }

    [Theory]
    [InlineData(PriorAuthStatus.Draft)]
    [InlineData(PriorAuthStatus.Submitted)]
    [InlineData(PriorAuthStatus.Approved)]
    [InlineData(PriorAuthStatus.Denied)]
    public void AppealDeny_FromInvalidStatus_ShouldThrow(PriorAuthStatus invalidFrom)
    {
        var request = CreateDraftRequest();
        SetStatus(request, invalidFrom);

        var act = () => request.AppealDeny("reviewer", "notes");

        act.Should().Throw<InvalidStatusTransitionException>()
            .Where(ex => ex.FromStatus == invalidFrom && ex.ToStatus == PriorAuthStatus.AppealDenied);
    }

    [Fact]
    public void ExpireIfOverdue_WhenNotOverdue_ShouldNotChangeStatus()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.Submitted);

        request.ExpireIfOverdue();

        request.Status.Should().Be(PriorAuthStatus.Submitted);
    }

    // --- Domain Events ---

    [Fact]
    public void Submit_ShouldAddPriorAuthSubmittedEvent()
    {
        var request = CreateDraftRequest();

        request.Submit();

        request.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PriorAuthSubmittedEvent>();
    }

    [Fact]
    public void Approve_ShouldAddPriorAuthApprovedEvent()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.UnderReview);

        request.Approve("reviewer-1", "Looks good.");

        request.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PriorAuthApprovedEvent>();
    }

    [Fact]
    public void Deny_ShouldAddPriorAuthDeniedEvent()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.UnderReview);

        request.Deny("reviewer-1", DenialReason.NotMedicallyNecessary, "Denied.");

        request.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PriorAuthDeniedEvent>();
    }

    [Fact]
    public void RequestAdditionalInfo_ShouldAddAdditionalInfoRequestedEvent()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.UnderReview);

        request.RequestAdditionalInfo("reviewer-1", "Need more info.");

        request.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AdditionalInfoRequestedEvent>();
    }

    [Fact]
    public void Appeal_ShouldAddPriorAuthAppealedEvent()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.Denied);

        request.Appeal("provider-1", "New evidence.");

        request.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PriorAuthAppealedEvent>();
    }

    [Fact]
    public void AppealApprove_ShouldAddPriorAuthAppealApprovedEvent()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.Appealed);

        request.AppealApprove("reviewer-2", "Granted.");

        request.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PriorAuthAppealApprovedEvent>();
    }

    [Fact]
    public void AppealDeny_ShouldAddPriorAuthAppealDeniedEvent()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.Appealed);

        request.AppealDeny("reviewer-2", "Still denied.");

        request.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PriorAuthAppealDeniedEvent>();
    }

    [Fact]
    public void ExpireIfOverdue_ShouldAddPriorAuthExpiredEvent()
    {
        var request = CreateDraftRequest();
        SetStatus(request, PriorAuthStatus.Submitted);
        SetRequiredResponseBy(request, DateTime.UtcNow.AddDays(-1));

        request.ExpireIfOverdue();

        request.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PriorAuthExpiredEvent>();
    }

    // --- Status Transitions History ---

    [Fact]
    public void ValidTransition_ShouldAddStatusTransitionRecord()
    {
        var request = CreateDraftRequest();

        request.Submit();

        request.StatusTransitions.Should().ContainSingle();
        request.StatusTransitions[0].FromStatus.Should().Be(PriorAuthStatus.Draft);
        request.StatusTransitions[0].ToStatus.Should().Be(PriorAuthStatus.Submitted);
    }
}
