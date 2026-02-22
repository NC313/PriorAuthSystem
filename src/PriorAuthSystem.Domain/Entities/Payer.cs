using PriorAuthSystem.Domain.Common;
using PriorAuthSystem.Domain.ValueObjects;

namespace PriorAuthSystem.Domain.Entities;

public class Payer : BaseEntity
{
    public string PayerName { get; private set; }
    public string PayerId { get; private set; }
    public int StandardResponseDays { get; private set; }
    public ContactInfo ContactInfo { get; private set; }

    private Payer() { }

    public Payer(
        string payerName,
        string payerId,
        int standardResponseDays,
        ContactInfo contactInfo)
    {
        if (string.IsNullOrWhiteSpace(payerName))
            throw new ArgumentException("Payer name cannot be empty.", nameof(payerName));

        if (string.IsNullOrWhiteSpace(payerId))
            throw new ArgumentException("Payer ID cannot be empty.", nameof(payerId));

        if (standardResponseDays <= 0)
            throw new ArgumentException("Standard response days must be greater than zero.", nameof(standardResponseDays));

        PayerName = payerName.Trim();
        PayerId = payerId.Trim();
        StandardResponseDays = standardResponseDays;
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
    }
}