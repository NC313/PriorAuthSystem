using FluentAssertions;
using PriorAuthSystem.Domain.Enums;
using PriorAuthSystem.Domain.Exceptions;
using PriorAuthSystem.Tests.Helpers;

namespace PriorAuthSystem.Tests.Domain;

public class PriorAuthorizationRequestTests
{
    // ── Submit ───────────────────────────────────────────────────────────────

    [Fact]
    public void Submit_FromDraft_SetsStatusToSubmitted()
    {
        var request = PriorAuthFactory.CreateDraft();

        request.Submit();

        request.Status.Should().Be(PriorAuthStatus.Submitted);
    }

    [Fact]
    public void Submit_FromDraft_AddsStatusTransition()
    {
        var request = PriorAuthFactory.CreateDraft();

        request.Submit();

        request.StatusTransitions.Should().ContainSingle()
            .Which.ToStatus.Should().Be(PriorAuthStatus.Submitted);
    }

    [Fact]
    public void Submit_FromDraft_RaisesSubmittedDomainEvent()
    {
        var request = PriorAuthFactory.CreateDraft();

        request.Submit();

        request.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PriorAuthSubmittedEvent>();
    }

    [Fact]
    public void Submit_FromApproved_ThrowsInvalidStatusTransitionException()
    {
        var request = PriorAuthFactory.CreateSubmitted();
        request.Approve("reviewer", "ok");

        var act = () => request.Submit();

        act.Should().Throw<InvalidStatusTransitionException>()
            .WithMessage("*Approved*Submitted*");
    }

    // ── Approve ──────────────────────────────────────────────────────────────

    [Fact]
    public void Approve_FromSubmitted_SetsStatusToApproved()
    {
        var request = PriorAuthFactory.CreateSubmitted();

        request.Approve("reviewer-1", "Clinically appropriate.");

        request.Status.Should().Be(PriorAuthStatus.Approved);
    }

    [Fact]
    public void Approve_FromDraft_ThrowsInvalidStatusTransitionException()
    {
        var request = PriorAuthFactory.CreateDraft();

        var act = () => request.Approve("reviewer-1", "notes");

        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void Approve_RecordsReviewerInTransition()
    {
        var request = PriorAuthFactory.CreateSubmitted();

        request.Approve("reviewer-42", "Approved.");

        request.StatusTransitions.Last().TransitionedBy.Should().Be("reviewer-42");
    }

    // ── Deny ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Deny_FromSubmitted_SetsStatusToDenied()
    {
        var request = PriorAuthFactory.CreateSubmitted();

        request.Deny("reviewer-1", DenialReason.NotMedicallyNecessary, "Not medically necessary.");

        request.Status.Should().Be(PriorAuthStatus.Denied);
    }

    [Fact]
    public void Deny_FromApproved_ThrowsInvalidStatusTransitionException()
    {
        var request = PriorAuthFactory.CreateSubmitted();
        request.Approve("reviewer-1", "ok");

        var act = () => request.Deny("reviewer-1", DenialReason.Other, "Oops");

        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void Deny_EmbedsDenialReasonInTransitionNotes()
    {
        var request = PriorAuthFactory.CreateSubmitted();

        request.Deny("reviewer-1", DenialReason.OutOfNetwork, "Provider not in network.");

        request.StatusTransitions.Last().Notes.Should().Contain("OutOfNetwork");
    }

    // ── RequestAdditionalInfo ─────────────────────────────────────────────────

    [Fact]
    public void RequestAdditionalInfo_FromSubmitted_SetsCorrectStatus()
    {
        var request = PriorAuthFactory.CreateSubmitted();

        request.RequestAdditionalInfo("reviewer-1", "Please supply imaging results.");

        request.Status.Should().Be(PriorAuthStatus.AdditionalInfoRequested);
    }

    // ── Appeal ────────────────────────────────────────────────────────────────

    [Fact]
    public void Appeal_FromDenied_SetsStatusToAppealed()
    {
        var request = PriorAuthFactory.CreateDenied();

        request.Appeal("provider-1", "New clinical evidence provided.");

        request.Status.Should().Be(PriorAuthStatus.Appealed);
    }

    [Fact]
    public void Appeal_FromSubmitted_ThrowsInvalidStatusTransitionException()
    {
        var request = PriorAuthFactory.CreateSubmitted();

        var act = () => request.Appeal("provider-1", "justification");

        act.Should().Throw<InvalidStatusTransitionException>();
    }

    // ── ExpireIfOverdue ───────────────────────────────────────────────────────

    [Fact]
    public void ExpireIfOverdue_WhenPastDeadline_SetsStatusToExpired()
    {
        var request = PriorAuthFactory.CreateOverdue();

        request.ExpireIfOverdue();

        request.Status.Should().Be(PriorAuthStatus.Expired);
    }

    [Fact]
    public void ExpireIfOverdue_WhenNotYetDue_DoesNotChangeStatus()
    {
        var request = PriorAuthFactory.CreateSubmitted(); // deadline is 14 days out

        request.ExpireIfOverdue();

        request.Status.Should().Be(PriorAuthStatus.Submitted);
    }

    [Fact]
    public void ExpireIfOverdue_WhenAlreadyApproved_ThrowsInvalidStatusTransitionException()
    {
        var request = PriorAuthFactory.CreateSubmitted();
        request.Approve("reviewer", "ok");

        // Force overdue doesn't matter — Approved is not a valid from-state for Expire
        var act = () => request.ExpireIfOverdue();

        // ExpireIfOverdue returns early when not overdue, so manipulate deadline via overdue factory
        // then approve to confirm Approved cannot expire
        var overdue = PriorAuthFactory.CreateOverdue();
        overdue.Approve("reviewer", "ok");
        var act2 = () => overdue.ExpireIfOverdue();

        act2.Should().Throw<InvalidStatusTransitionException>();
    }
}
