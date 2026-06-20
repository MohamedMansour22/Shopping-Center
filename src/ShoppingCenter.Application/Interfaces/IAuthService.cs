using ShoppingCenter.Application.DTOs;

namespace ShoppingCenter.Application.Interfaces;

public interface IAuthService
{
    /// <summary>Validates credentials and returns a JWT, or null if authentication fails.</summary>
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}
