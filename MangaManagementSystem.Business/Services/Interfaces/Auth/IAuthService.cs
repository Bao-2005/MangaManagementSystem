using MangaManagementSystem.Business.DTOs.Requests.Auth;
using MangaManagementSystem.Business.DTOs.Responses.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.Services.Interfaces.Auth
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        Task<(AuthResponse User, string AccessToken, string RefreshToken)> LoginAsync(LoginRequest request);

        Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);

        Task LogoutAsync(Guid userId);

        Task<AuthResponse> GetCurrentUserAsync(Guid userId);

        Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    }
}
