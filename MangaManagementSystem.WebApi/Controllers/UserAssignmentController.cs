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
    [Produces("application/json")]
    [Tags("UserAssignments")]
    public class UserAssignmentController : ControllerBase
    {
        private readonly IUserAssignmentService _service;
        public UserAssignmentController(IUserAssignmentService service) => _service = service;

        [HttpGet("api/user-assignments/from-me")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Get my assigned Tantou Editor (Mangaka only)")]
        public async Task<IActionResult> GetFromMe()
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse { Data = await _service.GetByMangakaAsync(userId), Message = "Success" });
        }

        [HttpGet("api/user-assignments/to-me")]
        [Authorize(Policy = "TantouEditorOnly")]
        [SwaggerOperation(Summary = "Get Mangaka(s) assigned to me (Tantou Editor only)")]
        public async Task<IActionResult> GetToMe()
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse { Data = await _service.GetByTantouEditorAsync(userId), Message = "Success" });
        }

        [HttpPost("api/user-assignments")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Assign an assistant (Mangaka only)")]
        public async Task<IActionResult> Create([FromBody] CreateUserAssignmentRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _service.CreateAsync(userId, request);
            return Ok(new BaseResponse { Data = result, Message = "Assignment created." });
        }

        [HttpPut("api/user-assignments/{id:guid}/unassign")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Unassign an assistant (Mangaka only)")]
        public async Task<IActionResult> Unassign(Guid id)
        {
            await _service.UnassignAsync(id);
            return Ok(new BaseResponse { Message = "Unassigned." });
        }

        [HttpDelete("api/user-assignments/{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Soft-delete a user assignment")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _service.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Deleted." });
        }

        private Guid? GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out var id) ? id : null;
        }
    }
}
