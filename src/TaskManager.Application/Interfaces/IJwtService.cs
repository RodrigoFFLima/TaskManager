using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}
