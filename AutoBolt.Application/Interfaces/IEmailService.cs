namespace AutoBolt.Application.Interfaces;

public interface IEmailService
{
    Task SendStaffCredentialsAsync(string toEmail, string fullName, string password);
}
