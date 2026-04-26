using AutoBolt.Application.Common.Interfaces;
using AutoBolt.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AutoBolt.Application.Features.Auth.Commands.RegisterCustomer;

public record RegisterCustomerCommand(
    string FullName,
    string Email,
    string Password,
    string Phone,
    string? Address
) : IRequest<bool>;

public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IEmailService _emailService;

    public RegisterCustomerCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;
    }

    public async Task<bool> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new Exception("User with this email already exists.");
        }

        // Create the user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            CustomerDetails = new Customer
            {
                FullName = request.FullName,
                Phone = request.Phone,
                Address = request.Address,
                Email = request.Email
            }
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"User creation failed: {errors}");
        }

        // Ensure role exists and assign it
        if (!await _roleManager.RoleExistsAsync("Customer"))
        {
            await _roleManager.CreateAsync(new ApplicationRole("Customer"));
        }
        await _userManager.AddToRoleAsync(user, "Customer");

        // Generate 6-digit OTP
        var otp = Random.Shared.Next(100000, 999999).ToString();
        user.EmailConfirmationOTP = otp;
        user.OTPExpiryTime = DateTime.UtcNow.AddMinutes(15);
        
        await _userManager.UpdateAsync(user);
        
        var body = $"Your verification code is: {otp}. It will expire in 15 minutes.";
        await _emailService.SendEmailAsync(user.Email, "Confirm your email", body);

        return true;
    }
}
