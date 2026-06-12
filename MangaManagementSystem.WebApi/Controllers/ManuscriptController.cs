using MangaManagementSystem.Business.DTOs.Requests.Manuscripts;
using MangaManagementSystem.Business.Services.Interfaces.Manuscripts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Tags("Manuscripts")]
    public class ManuscriptController : ControllerBase
    {
        private readonly IManuscriptService _service;
        public ManuscriptController(IManuscriptService service) => _service = service;

        [HttpGet("api/chapters/{chapterId:guid}/manuscripts")]
        [Authorize]
        [SwaggerOperation(Summary = "Get manuscripts by chapter")]
        public async Task<IActionResult> GetByChapter(Guid chapterId)
            => Ok(new BaseResponse { Data = await _service.GetByChapterAsync(chapterId), Message = "Success" });

        [HttpGet("api/manuscripts/{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get manuscript by ID")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(new BaseResponse { Data = await _service.GetByIdAsync(id), Message = "Success" });

        [HttpPost("api/manuscripts")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Submit a manuscript (Mangaka only; all page tasks must be approved)")]
        public async Task<IActionResult> Create([FromBody] CreateManuscriptRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _service.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = result.ManuscriptId }, new BaseResponse { Data = result, Message = "Manuscript submitted." });
        }

        [HttpPut("api/manuscripts/{id:guid}")]
        [Authorize(Policy = "TantouEditorOnly")]
        [SwaggerOperation(Summary = "Review / update manuscript status (TantouEditor only)")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateManuscriptRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse { Data = await _service.UpdateAsync(id, request, userId), Message = "Updated." });
        }

        [HttpDelete("api/manuscripts/{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Soft-delete a manuscript")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _service.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Manuscript deleted." });
        }

        private Guid? GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out var id) ? id : null;
        }
    }
}
