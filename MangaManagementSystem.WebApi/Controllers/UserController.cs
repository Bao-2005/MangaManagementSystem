using MangaManagementSystem.Business.DTOs.Requests.Users;
using MangaManagementSystem.Business.Services.Interfaces.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    [Tags("Users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Get all users",
            Description = "Admin-only. Returns all non-deleted users with their profile information.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(new BaseResponse { Data = users, Message = "Success" });
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Update a user account",
            Description = "Admin-only. Updates account profile fields, role, and optionally resets the password.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(Guid id, [FromBody] AdminUpdateUserRequest request)
        {
            var user = await _userService.AdminUpdateAsync(id, request);
            return Ok(new BaseResponse { Data = user, Message = "User updated successfully." });
        }

        [HttpPut("me")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Update my profile",
            Description = "Updates the authenticated user's basic profile fields only.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateMyProfileRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var user = await _userService.UpdateMyProfileAsync(userId, request);
            return Ok(new BaseResponse { Data = user, Message = "Profile updated successfully." });
        }

        [HttpDelete("{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Soft-delete a user",
            Description = "Admin-only. Sets DeletedAt to the current UTC time on the user and cascades to their UserAssignments, PageTasks, Annotations, FileAssets, and Series.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _userService.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "User deleted successfully." });
        }

        [HttpGet("my-mangakas")]
        [Authorize(Policy = "TantouEditorOnly")]
        [SwaggerOperation(
            Summary = "Get assigned mangakas for the current editor",
            Description = "Tantou Editor only. Returns all active Mangakas assigned to the calling editor.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyMangakas()
        {
            var editorId = GetUserId() ?? throw new UnauthorizedAccessException();
            var mangakas = await _userService.GetAssignedMangakasAsync(editorId);
            return Ok(new BaseResponse { Data = mangakas, Message = "Success" });
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }
    }
}
