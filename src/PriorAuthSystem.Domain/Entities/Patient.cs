using PriorAuthSystem.Domain.Common;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Domain.Entities;

public class Patient : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public string MemberId { get; private set; }
    public string InsurancePlanId { get; private set; }
    public ContactInfo ContactInfo { get; private set; }

    public string FullName => $"{FirstName} {LastName}";
    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year;

    private Patient() { }

    public Patient(
        string firstName,
        string lastName,
        DateTime dateOfBirth,
        string memberId,
        string insurancePlanId,
        ContactInfo contactInfo)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        if (string.IsNullOrWhiteSpace(memberId))
            throw new ArgumentException("Member ID cannot be empty.", nameof(memberId));

        if (dateOfBirth >= DateTime.UtcNow)
            throw new ArgumentException("Date of birth must be in the past.", nameof(dateOfBirth));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        DateOfBirth = dateOfBirth;
        MemberId = memberId.Trim();
        InsurancePlanId = insurancePlanId?.Trim() ?? string.Empty;
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
    }

    public void UpdateContactInfo(ContactInfo contactInfo)
    {
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        SetUpdated();
    }
}