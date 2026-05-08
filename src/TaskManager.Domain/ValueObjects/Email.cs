using TaskManager.Domain.Exceptions;

namespace TaskManager.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        var trimmed = value.Trim().ToLowerInvariant();

        if (trimmed.Length > 254)
            throw new DomainException("Email is too long.");

        if (!trimmed.Contains('@') || !trimmed.Contains('.'))
            throw new DomainException("Email format is invalid.");

        return new Email(trimmed);
    }

    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Email email && Equals(email);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
