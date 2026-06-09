using MangaManagementSystem.Business.DTOs.Requests.Chapters;
using MangaManagementSystem.Business.Services.Interfaces.Chapters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Tags("Chapters")]
    public class ChapterController : ControllerBase
    {
        private readonly IChapterService _service;
        public ChapterController(IChapterService service) => _service = service;

        [HttpGet("api/chapters")]
        [Authorize]
        [SwaggerOperation(Summary = "Get all chapters")]
        public async Task<IActionResult> GetAll()
            => Ok(new BaseResponse { Data = await _service.GetAllAsync(), Message = "Success" });

        [HttpGet("api/series/{seriesId:guid}/chapters")]
        [Authorize]
        [SwaggerOperation(Summary = "Get chapters by series")]
        public async Task<IActionResult> GetBySeries(Guid seriesId)
            => Ok(new BaseResponse { Data = await _service.GetBySeriesAsync(seriesId), Message = "Success" });

        [HttpGet("api/chapters/{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get chapter by ID")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(new BaseResponse { Data = await _service.GetByIdAsync(id), Message = "Success" });

        [HttpPost("api/chapters")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Create a chapter")]
        public async Task<IActionResult> Create([FromBody] CreateChapterRequest request)
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.ChapterId }, new BaseResponse { Data = result, Message = "Chapter created." });
        }

        [HttpPut("api/chapters/{id:guid}")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Update a chapter")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChapterRequest request)
            => Ok(new BaseResponse { Data = await _service.UpdateAsync(id, request), Message = "Updated." });

        [HttpDelete("api/chapters/{id:guid}/soft-delete")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Soft-delete a chapter")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _service.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Chapter deleted." });
        }
    }
}
