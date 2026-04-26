using AutoBolt.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AutoBolt.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(string Email, string OTP) : IRequest<bool>;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ConfirmEmailCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        if (user.EmailConfirmationOTP != request.OTP)
        {
            throw new Exception("Invalid verification code.");
        }

        if (user.OTPExpiryTime < DateTime.UtcNow)
        {
            throw new Exception("Verification code has expired.");
        }

        user.EmailConfirmed = true;
        user.EmailConfirmationOTP = null;
        user.OTPExpiryTime = null;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}
