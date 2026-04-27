using System.ComponentModel.DataAnnotations;
using AutoBolt.Application.Common.Interfaces;
using AutoBolt.Application.Features.Auth.Common;
using AutoBolt.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AutoBolt.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<AuthResponse>
{
    [Required(ErrorMessage = "Access token is required.")]
    public string AccessToken { get; init; } = string.Empty;

    [Required(ErrorMessage = "Refresh token is required.")]
    public string RefreshToken { get; init; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);
        
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new Exception("Invalid or expired refresh token.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name!).ToList();
        var newAccessToken = _jwtTokenGenerator.GenerateToken(user, roles);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(double.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"]!));

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.FullName,
            user.Email!,
            newAccessToken,
            newRefreshToken,
            user.RefreshTokenExpiryTime.Value
        );
    }
}
