using PriorAuthSystem.Domain.Common;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Domain.Entities;

public class Provider : BaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string NPI { get; private set; }
    public string Specialty { get; private set; }
    public string OrganizationName { get; private set; }
    public ContactInfo ContactInfo { get; private set; }

    public string FullName => $"Dr. {FirstName} {LastName}";

    private Provider() { }

    public Provider(
        string firstName,
        string lastName,
        string npi,
        string specialty,
        string organizationName,
        ContactInfo contactInfo)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        if (string.IsNullOrWhiteSpace(npi) || npi.Length != 10 || !npi.All(char.IsDigit))
            throw new ArgumentException("NPI must be exactly 10 digits.", nameof(npi));

        if (string.IsNullOrWhiteSpace(specialty))
            throw new ArgumentException("Specialty cannot be empty.", nameof(specialty));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        NPI = npi.Trim();
        Specialty = specialty.Trim();
        OrganizationName = organizationName?.Trim() ?? string.Empty;
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
    }

    public void UpdateContactInfo(ContactInfo contactInfo)
    {
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        SetUpdated();
    }
}