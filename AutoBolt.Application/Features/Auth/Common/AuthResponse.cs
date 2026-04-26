namespace AutoBolt.Application.Features.Auth.Common;

public record AuthResponse(
    Guid Id,
    string FullName,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime Expiry
);
