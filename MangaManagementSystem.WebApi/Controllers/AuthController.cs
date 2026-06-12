using MangaManagementSystem.Business.DTOs.Requests.Auth;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.DTOs.Responses.Auth;
using MangaManagementSystem.Business.DTOs.Responses.Users;
using MangaManagementSystem.Business.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    [Tags("Authentication")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Create user account",
            Description = "Admin-only endpoint for creating user accounts. Provide username, email, display name, password, target role ID, and optional source assignment user ID. Mangaka accounts require an active Tantou Editor as the source assignment user.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var authResponse = await _authService.RegisterAsync(request);

            var data = new UserDto
            {
                Id = authResponse.UserId.ToString(),
                Email = authResponse.Email,
                Name = authResponse.DisplayName,
                Role = authResponse.RoleName
            };

            return Ok(new BaseResponse { Data = data, Message = "Success" });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Login",
            Description = "Authenticate with email and password. Returns a JWT access token (15 min), a refresh token (7 days), and the authenticated user's profile.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (authResponse, accessToken, refreshToken) = await _authService.LoginAsync(request);

            var data = new LoginDataDto
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = authResponse.UserId.ToString(),
                    Email = authResponse.Email,
                    Name = authResponse.DisplayName,
                    Role = authResponse.RoleName
                }
            };

            return Ok(new BaseResponse { Data = data, Message = "Success" });
        }

        [HttpGet("me")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Get current user",
            Description = "Returns the profile of the authenticated user based on the Bearer token in the Authorization header.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Me()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            var authResponse = await _authService.GetCurrentUserAsync(userId.Value);

            var data = new UserDto
            {
                Id = authResponse.UserId.ToString(),
                Email = authResponse.Email,
                Name = authResponse.DisplayName,
                Role = authResponse.RoleName
            };

            return Ok(new BaseResponse { Data = data, Message = "Success" });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [SwaggerOperation(
            Summary = "Refresh tokens",
            Description = "Exchanges a valid refresh token for a new access token and refresh token. The old refresh token is immediately invalidated (single-use rotation).")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var (newAccessToken, newRefreshToken) = await _authService.RefreshTokenAsync(request.RefreshToken);

            var data = new RefreshDataDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            };

            return Ok(new BaseResponse { Data = data, Message = "Success" });
        }

        [HttpPost("logout")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Logout",
            Description = "Invalidates the current refresh token in the database. The access token will expire naturally after 15 minutes.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            await _authService.LogoutAsync(userId.Value);

            return Ok(new BaseResponse { Message = "Success" });
        }

        [HttpPut("change-password")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Change password",
            Description = "Changes the authenticated user's password. Requires the current password for verification. All active sessions are invalidated — the user must log in again with the new password.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            await _authService.ChangePasswordAsync(userId.Value, request);

            return Ok(new BaseResponse { Message = "Success" });
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }
    }
}
