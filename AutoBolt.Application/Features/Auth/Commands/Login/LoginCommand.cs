using System.ComponentModel.DataAnnotations;
using AutoBolt.Application.Common.Interfaces;
using AutoBolt.Application.Features.Auth.Common;
using AutoBolt.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AutoBolt.Application.Features.Auth.Commands.Login;

public record LoginCommand : IRequest<AuthResponse>
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; init; } = string.Empty;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
        {
            throw new Exception("Invalid credentials.");
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new Exception("Invalid credentials.");
        }

        if (!user.EmailConfirmed)
        {
            throw new Exception("Email not confirmed.");
        }

        if (!user.IsActive)
        {
            throw new Exception("Account is inactive.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name!).ToList();
        var accessToken = _jwtTokenGenerator.GenerateToken(user, roles);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(double.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"]!));

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.FullName,
            user.Email!,
            accessToken,
            refreshToken,
            user.RefreshTokenExpiryTime.Value
        );
    }
}
