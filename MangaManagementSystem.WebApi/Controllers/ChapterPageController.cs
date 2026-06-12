using MangaManagementSystem.Business.DTOs.Requests.Chapters;
using MangaManagementSystem.Business.Services.Interfaces.Chapters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/chapters/{chapterId:guid}/pages")]
    [Produces("application/json")]
    [Tags("ChapterPages")]
    public class ChapterPageController : ControllerBase
    {
        private readonly IChapterPageService _service;
        public ChapterPageController(IChapterPageService service) => _service = service;

        [HttpGet]
        [Authorize]
        [SwaggerOperation(Summary = "Get all pages for a chapter")]
        public async Task<IActionResult> GetByChapter(Guid chapterId)
            => Ok(new BaseResponse { Data = await _service.GetByChapterAsync(chapterId), Message = "Success" });

        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get a chapter page by ID")]
        public async Task<IActionResult> GetById(Guid chapterId, Guid id)
            => Ok(new BaseResponse { Data = await _service.GetByIdAsync(id), Message = "Success" });

        [HttpPost]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Add a final page to a chapter")]
        public async Task<IActionResult> Create(Guid chapterId, [FromBody] CreateChapterPageRequest request)
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { chapterId, id = result.ChapterPageId }, new BaseResponse { Data = result, Message = "Created." });
        }

        [HttpDelete("{id:guid}/soft-delete")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Soft-delete a chapter page")]
        public async Task<IActionResult> SoftDelete(Guid chapterId, Guid id)
        {
            await _service.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Deleted." });
        }
    }
}
