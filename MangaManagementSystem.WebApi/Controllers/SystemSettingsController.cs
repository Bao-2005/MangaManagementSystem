using MangaManagementSystem.Business.DTOs.Requests.Settings;
using MangaManagementSystem.Business.Services.Interfaces.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/settings")]
    [Produces("application/json")]
    [Tags("System Settings")]
    public class SystemSettingsController : ControllerBase
    {
        private readonly ISystemSettingService _systemSettingService;

        public SystemSettingsController(ISystemSettingService systemSettingService)
        {
            _systemSettingService = systemSettingService;
        }

        [HttpGet("page-task/max-submission-attempts")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Get max submission attempts setting",
            Description = "Admin-only. Returns the configured maximum number of submission attempts for a page task. If no setting exists, returns the default value.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMaxSubmissionAttempts()
        {
            var setting = await _systemSettingService.GetMaxSubmissionAttemptsAsync();
            return Ok(new BaseResponse { Data = setting, Message = "Success" });
        }

        [HttpPut("page-task/max-submission-attempts")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(
            Summary = "Create or update max submission attempts setting",
            Description = "Admin-only. Creates the setting if it does not exist, otherwise updates it.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpsertMaxSubmissionAttempts([FromBody] UpdateMaxSubmissionAttemptsRequest request)
        {
            var setting = await _systemSettingService.UpsertMaxSubmissionAttemptsAsync(request.Value);
            return Ok(new BaseResponse { Data = setting, Message = "Updated." });
        }
    }
}
