using Microsoft.AspNetCore.Identity;
using ShoppingCenter.Application.DTOs;
using ShoppingCenter.Application.Interfaces;

namespace ShoppingCenter.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public AuthService(UserManager<ApplicationUser> userManager, TokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return null;

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAt) = _tokenService.CreateToken(user, roles);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            ExpiresAtUtc = expiresAt
        };
    }
}
