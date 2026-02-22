namespace PriorAuthSystem.Domain.ValueObjects;

public sealed class IcdCode
{
    public string Code { get; }
    public string Description { get; }

    public IcdCode(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("ICD-10 code cannot be empty.", nameof(code));

        if (code.Length < 3 || code.Length > 7)
            throw new ArgumentException("ICD-10 code must be between 3 and 7 characters.", nameof(code));

        Code = code.ToUpper().Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    public override string ToString() => $"{Code} - {Description}";

    public override bool Equals(object? obj) =>
        obj is IcdCode other && Code == other.Code;

    public override int GetHashCode() => Code.GetHashCode();
}