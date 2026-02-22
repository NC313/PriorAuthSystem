namespace PriorAuthSystem.Domain.ValueObjects;

public sealed class CptCode
{
    public string Code { get; }
    public string Description { get; }
    public bool RequiresPriorAuth { get; }

    public CptCode(string code, string description, bool requiresPriorAuth = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("CPT code cannot be empty.", nameof(code));

        if (code.Length != 5)
            throw new ArgumentException("CPT code must be exactly 5 characters.", nameof(code));

        Code = code.Trim();
        Description = description?.Trim() ?? string.Empty;
        RequiresPriorAuth = requiresPriorAuth;
    }

    public override string ToString() => $"{Code} - {Description}";

    public override bool Equals(object? obj) =>
        obj is CptCode other && Code == other.Code;

    public override int GetHashCode() => Code.GetHashCode();
}