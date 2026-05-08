using TaskManager.Domain.Exceptions;
using TaskManager.Domain.ValueObjects;

namespace TaskManager.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private User() { }

    public static User Create(string name, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name cannot be empty.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash cannot be empty.");

        return new User
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Email = Email.Create(email),
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static User Reconstitute(Guid id, string name, string email, string passwordHash,
        DateTime createdAt, DateTime updatedAt) => new()
    {
        Id = id,
        Name = name,
        Email = Email.Create(email),
        PasswordHash = passwordHash,
        CreatedAt = createdAt,
        UpdatedAt = updatedAt
    };

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name cannot be empty.");

        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
