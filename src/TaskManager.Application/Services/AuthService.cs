using FluentValidation;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Validators;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly RegisterValidator _registerValidator;
    private readonly LoginValidator _loginValidator;

    public AuthService(IUserRepository userRepository, IJwtService jwtService, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _registerValidator = new RegisterValidator();
        _loginValidator = new LoginValidator();
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var validation = await _registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        if (await _userRepository.ExistsByEmailAsync(request.Email, ct))
            throw new DomainException("Email is already registered.");

        var hash = _passwordHasher.Hash(request.Password);
        var user = User.Create(request.Name, request.Email, hash);

        await _userRepository.AddAsync(user, ct);

        var token = _jwtService.GenerateToken(user);
        return new AuthResponse(token, user.Name, user.Email.Value, user.Id);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var validation = await _loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var user = await _userRepository.GetByEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        var token = _jwtService.GenerateToken(user);
        return new AuthResponse(token, user.Name, user.Email.Value, user.Id);
    }
}
