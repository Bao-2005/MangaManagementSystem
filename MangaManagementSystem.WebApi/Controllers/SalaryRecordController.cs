using MangaManagementSystem.Business.Services.Interfaces.SalaryRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/salary-records")]
    [Produces("application/json")]
    [Tags("Salary Records")]
    public class SalaryRecordController : ControllerBase
    {
        private readonly ISalaryRecordService _salaryRecordService;

        public SalaryRecordController(ISalaryRecordService salaryRecordService)
        {
            _salaryRecordService = salaryRecordService;
        }

        [HttpGet]
        [Authorize]
        [SwaggerOperation(
            Summary = "Get salary records",
            Description = "Admins can view all salary records. Mangakas can view records for tasks in their series. Assistants can view only their own records.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get([FromQuery] Guid? assistantId = null)
        {
            var requesterId = GetUserId() ?? throw new UnauthorizedAccessException();
            var requesterRole = User.FindFirstValue(ClaimTypes.Role)
                ?? throw new UnauthorizedAccessException("Role claim is missing.");

            var records = await _salaryRecordService.GetAsync(requesterId, requesterRole, assistantId);
            return Ok(new BaseResponse { Data = records, Message = "Success" });
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }
    }
}
