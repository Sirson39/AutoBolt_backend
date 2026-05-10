namespace AutoBolt.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(int userId, string email, string fullName, string role, int? customerId = null);
}
