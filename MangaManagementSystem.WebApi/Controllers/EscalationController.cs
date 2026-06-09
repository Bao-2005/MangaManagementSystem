using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Tags("Escalations")]
    public class EscalationController : ControllerBase
    {
        private readonly IEscalationService _service;
        public EscalationController(IEscalationService service) => _service = service;

        [HttpGet("api/series/{seriesId:guid}/escalations")]
        [Authorize]
        [SwaggerOperation(Summary = "Get escalations for a series")]
        public async Task<IActionResult> GetBySeries(Guid seriesId)
            => Ok(new BaseResponse { Data = await _service.GetBySeriesAsync(seriesId), Message = "Success" });

        [HttpGet("api/escalations/{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get escalation by ID")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(new BaseResponse { Data = await _service.GetByIdAsync(id), Message = "Success" });

        [HttpPost("api/escalations")]
        [Authorize]
        [SwaggerOperation(Summary = "Raise an escalation")]
        public async Task<IActionResult> Create([FromBody] CreateEscalationRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _service.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = result.EscalationId },
                new BaseResponse { Data = result, Message = "Escalation raised." });
        }

        [HttpPut("api/escalations/{id:guid}/resolve")]
        [Authorize(Policy = "EditorInChiefOnly")]
        [SwaggerOperation(Summary = "Resolve an escalation (EditorInChief only)")]
        public async Task<IActionResult> Resolve(Guid id, [FromBody] UpdateEscalationRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _service.ResolveAsync(id, userId, request);
            return Ok(new BaseResponse { Data = result, Message = "Escalation resolved." });
        }

        [HttpDelete("api/escalations/{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Soft-delete an escalation")]
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
