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
    [Route("api/series")]
    [Produces("application/json")]
    [Tags("Series")]
    public class SeriesController : ControllerBase
    {
        private readonly ISeriesService _seriesService;
        public SeriesController(ISeriesService seriesService) => _seriesService = seriesService;

        [HttpGet]
        [Authorize]
        [SwaggerOperation(Summary = "Get all series", Description = "Optionally filter by status query param.")]
        public async Task<IActionResult> GetAll([FromQuery] string? status = null)
            => Ok(new BaseResponse { Data = await _seriesService.GetAllAsync(status), Message = "Success" });

        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get series detail by ID")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(new BaseResponse { Data = await _seriesService.GetByIdAsync(id), Message = "Success" });

        [HttpGet("mangaka/{mangakaId:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get series by Mangaka ID")]
        public async Task<IActionResult> GetByMangaka(Guid mangakaId)
            => Ok(new BaseResponse { Data = await _seriesService.GetByMangakaAsync(mangakaId), Message = "Success" });

        [HttpPost]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Create a series proposal (Mangaka only)")]
        public async Task<IActionResult> Create([FromBody] CreateSeriesRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _seriesService.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = result.SeriesId }, new BaseResponse { Data = result, Message = "Series proposed." });
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Update editable series fields")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSeriesRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse { Data = await _seriesService.UpdateAsync(id, userId, request), Message = "Updated." });
        }

        [HttpDelete("{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Soft-delete a series")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _seriesService.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Series deleted." });
        }

        private Guid? GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out var id) ? id : null;
        }
    }
}
