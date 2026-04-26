using AutoBolt.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AutoBolt.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailCommand(string Email, string Token) : IRequest<bool>;

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

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        return result.Succeeded;
    }
}
