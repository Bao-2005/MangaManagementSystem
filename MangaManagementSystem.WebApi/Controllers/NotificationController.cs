using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Tags("Notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;
        public NotificationController(INotificationService service) => _service = service;

        [HttpGet("api/notifications/my")]
        [Authorize]
        [SwaggerOperation(Summary = "Get my notifications")]
        public async Task<IActionResult> GetMy()
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse { Data = await _service.GetMyNotificationsAsync(userId), Message = "Success" });
        }

        [HttpPut("api/notifications/{id:guid}/read")]
        [Authorize]
        [SwaggerOperation(Summary = "Mark a notification as read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            await _service.MarkAsReadAsync(id, userId);
            return Ok(new BaseResponse { Message = "Marked as read." });
        }

        [HttpPost("api/notifications")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Broadcast a notification to target users (Admin only)")]
        public async Task<IActionResult> Broadcast([FromBody] CreateNotificationRequest request)
        {
            var result = await _service.BroadcastAsync(request);
            return Ok(new BaseResponse { Data = result, Message = "Notification sent." });
        }

        [HttpDelete("api/notifications/{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Soft-delete a notification (Admin only)")]
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
