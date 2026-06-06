using MangaManagementSystem.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Tags("Rankings")]
    public class RankingSnapshotController : ControllerBase
    {
        private readonly IRankingSnapshotService _service;
        public RankingSnapshotController(IRankingSnapshotService service) => _service = service;

        [HttpGet("api/rankings")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Get all ranking snapshots",
            Description = "Optionally filter by period query param (e.g. '2026-05').")]
        public async Task<IActionResult> GetAll([FromQuery] string? period = null)
            => Ok(new BaseResponse { Data = await _service.GetAllByPeriodAsync(period), Message = "Success" });

        [HttpGet("api/series/{seriesId:guid}/rankings")]
        [Authorize]
        [SwaggerOperation(Summary = "Get ranking snapshots for a series")]
        public async Task<IActionResult> GetBySeries(Guid seriesId)
            => Ok(new BaseResponse { Data = await _service.GetBySeriesAsync(seriesId), Message = "Success" });

        [HttpDelete("api/rankings/{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Soft-delete a ranking snapshot")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _service.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Deleted." });
        }
    }
}
