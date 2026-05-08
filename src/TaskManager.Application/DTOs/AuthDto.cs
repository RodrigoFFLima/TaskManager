namespace TaskManager.Application.DTOs;

public record RegisterRequest(string Name, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record AuthResponse(string Token, string Name, string Email, Guid UserId);

public record UserDto(Guid Id, string Name, string Email, DateTime CreatedAt);
