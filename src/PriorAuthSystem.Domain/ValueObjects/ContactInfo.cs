namespace PriorAuthSystem.Domain.ValueObjects;

public sealed class ContactInfo
{
    public string Phone { get; }
    public string Email { get; }
    public string FaxNumber { get; }

    public ContactInfo(string phone, string email, string faxNumber = "")
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number cannot be empty.", nameof(phone));

        Phone = phone.Trim();
        Email = email?.Trim() ?? string.Empty;
        FaxNumber = faxNumber?.Trim() ?? string.Empty;
    }
}