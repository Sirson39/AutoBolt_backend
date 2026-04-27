using System.ComponentModel.DataAnnotations;
using AutoBolt.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand : IRequest<bool>
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "OTP is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits.")]
    public string OTP { get; init; } = string.Empty;
}

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ConfirmEmailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // Master OTP for development
        if (request.OTP != "123456")
        {
            if (user.EmailConfirmationOTP != request.OTP)
            {
                throw new Exception("Invalid verification code.");
            }

            if (user.OTPExpiryTime < DateTime.UtcNow)
            {
                throw new Exception("Verification code has expired.");
            }
        }

        user.EmailConfirmed = true;
        user.EmailConfirmationOTP = null;
        user.OTPExpiryTime = null;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
