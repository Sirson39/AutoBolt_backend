using AutoBolt.Application.Common.Interfaces;
using AutoBolt.Application.Features.Auth.Common;
using AutoBolt.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AutoBolt.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponse>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // In a strict implementation, you'd validate the expired access token too.
        // For now, we rely on the RefreshToken matching the user.
        
        var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);
        
        if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            throw new Exception("Invalid or expired refresh token.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtTokenGenerator.GenerateToken(user, roles);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(double.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"]!));

        await _userManager.UpdateAsync(user);

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
