using PriorAuthSystem.Domain.Entities;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Tests.Helpers;

/// <summary>
/// Factory methods for creating domain entities in specific states for testing.
/// </summary>
public static class PriorAuthFactory
{
    public static Patient CreatePatient() => new(
        firstName: "Jane",
        lastName: "Doe",
        dateOfBirth: new DateTime(1985, 6, 15),
        memberId: "MBR-001",
        insurancePlanId: "PLAN-001",
        contactInfo: new ContactInfo("555-000-0001", "jane.doe@example.com"));

    public static Provider CreateProvider() => new(
        firstName: "John",
        lastName: "Smith",
        npi: "1234567890",
        specialty: "Cardiology",
        organizationName: "Heart Clinic",
        contactInfo: new ContactInfo("555-000-0002", "dr.smith@clinic.com"));

    public static Payer CreatePayer() => new(
        payerName: "BlueCross",
        payerId: "BC-001",
        standardResponseDays: 14,
        contactInfo: new ContactInfo("555-000-0003", "auth@bluecross.com"));

    /// <summary>Creates a request in Draft status.</summary>
    public static PriorAuthorizationRequest CreateDraft(DateTime? requiredResponseBy = null) => new(
        patient: CreatePatient(),
        provider: CreateProvider(),
        payer: CreatePayer(),
        icdCode: new IcdCode("Z51.11", "Encounter for antineoplastic chemotherapy"),
        cptCode: new CptCode("96413", "Chemotherapy infusion", true),
        clinicalJustification: new ClinicalJustification(
            "Patient requires chemotherapy following breast cancer diagnosis.",
            "Dr. John Smith"),
        requiredResponseBy: requiredResponseBy ?? DateTime.UtcNow.AddDays(14));

    /// <summary>Creates a request in Submitted status.</summary>
    public static PriorAuthorizationRequest CreateSubmitted()
    {
        var request = CreateDraft();
        request.Submit();
        return request;
    }

    /// <summary>Creates a request in Denied status (via Submitted → Denied).</summary>
    public static PriorAuthorizationRequest CreateDenied()
    {
        var request = CreateSubmitted();
        request.Deny("reviewer-1", DenialReason.NotMedicallyNecessary, "Insufficient documentation.");
        return request;
    }

    /// <summary>Creates a request past its response deadline (used for expiry tests).</summary>
    public static PriorAuthorizationRequest CreateOverdue()
    {
        var request = CreateDraft(requiredResponseBy: DateTime.UtcNow.AddDays(-1));
        request.Submit();
        return request;
    }
}
