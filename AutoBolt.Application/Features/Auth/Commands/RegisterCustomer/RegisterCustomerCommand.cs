using System.ComponentModel.DataAnnotations;
using AutoBolt.Application.Common.Interfaces;
using AutoBolt.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Application.Features.Auth.Commands.RegisterCustomer;

public record RegisterCustomerCommand : IRequest<bool>
{
    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(100, ErrorMessage = "Full name must not exceed 100 characters.")]
    public string FullName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Phone number must be between 10 and 15 digits.")]
    public string Phone { get; init; } = string.Empty;

    public string? Address { get; init; }
}

public class RegisterCustomerCommandHandler : IRequestHandler<RegisterCustomerCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly IEmailService _emailService;

    public RegisterCustomerCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEmailService emailService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<bool> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (existingUser)
        {
            throw new Exception("User with this email already exists.");
        }

        // Create the user
        var user = new ApplicationUser
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.Phone,
            CustomerDetails = new Customer
            {
                FullName = request.FullName,
                Phone = request.Phone,
                Address = request.Address,
                Email = request.Email
            }
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        // Ensure role exists and assign it
        var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Customer", cancellationToken);
        if (customerRole == null)
        {
            customerRole = new ApplicationRole("Customer");
            _context.Roles.Add(customerRole);
            await _context.SaveChangesAsync(cancellationToken);
        }

        user.UserRoles.Add(new UserRole { User = user, Role = customerRole });

        // Generate 6-digit OTP
        var otp = Random.Shared.Next(100000, 999999).ToString();
        user.EmailConfirmationOTP = otp;
        user.OTPExpiryTime = DateTime.UtcNow.AddMinutes(15);
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        
        var body = $"Your verification code is: {otp}. It will expire in 15 minutes.";
        await _emailService.SendEmailAsync(user.Email, "Confirm your email", body);

        return true;
    }
}
