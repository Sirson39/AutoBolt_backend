namespace AutoBolt.Application.DTOs;

public class ConfirmSetupDto
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
