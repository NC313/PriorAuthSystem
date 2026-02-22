namespace PriorAuthSystem.Domain.ValueObjects;

public sealed class ClinicalJustification
{
    public string Notes { get; }
    public string SupportingDocumentPath { get; }
    public DateTime DocumentedAt { get; }
    public string DocumentedBy { get; }

    public ClinicalJustification(
        string notes,
        string documentedBy,
        string supportingDocumentPath = "")
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Clinical justification notes cannot be empty.", nameof(notes));

        if (string.IsNullOrWhiteSpace(documentedBy))
            throw new ArgumentException("Documenting provider cannot be empty.", nameof(documentedBy));

        Notes = notes.Trim();
        DocumentedBy = documentedBy.Trim();
        SupportingDocumentPath = supportingDocumentPath?.Trim() ?? string.Empty;
        DocumentedAt = DateTime.UtcNow;
    }

    public override string ToString() => $"Documented by {DocumentedBy} on {DocumentedAt:d}: {Notes}";
}