using MangaManagementSystem.Business.DTOs.Requests.Tasks;
using MangaManagementSystem.Business.Services.Interfaces.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/manuscripts/{manuscriptId:guid}/annotations")]
    [Produces("application/json")]
    [Tags("Annotations")]
    public class AnnotationController : ControllerBase
    {
        private readonly IAnnotationService _service;
        public AnnotationController(IAnnotationService service) => _service = service;

        [HttpGet]
        [Authorize]
        [SwaggerOperation(Summary = "Get all annotations for a manuscript")]
        public async Task<IActionResult> GetByManuscript(Guid manuscriptId)
            => Ok(new BaseResponse { Data = await _service.GetByManuscriptAsync(manuscriptId), Message = "Success" });

        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get annotation by ID")]
        public async Task<IActionResult> GetById(Guid manuscriptId, Guid id)
            => Ok(new BaseResponse { Data = await _service.GetByIdAsync(id), Message = "Success" });

        [HttpPost]
        [Authorize(Policy = "TantouEditorOnly")]
        [SwaggerOperation(Summary = "Add annotation to a manuscript page (TantouEditor only)")]
        public async Task<IActionResult> Create(Guid manuscriptId, [FromBody] CreateAnnotationRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _service.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { manuscriptId, id = result.AnnotationId }, new BaseResponse { Data = result, Message = "Annotation added." });
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "TantouEditorOnly")]
        [SwaggerOperation(Summary = "Update annotation content")]
        public async Task<IActionResult> Update(Guid manuscriptId, Guid id, [FromBody] UpdateAnnotationRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse { Data = await _service.UpdateAsync(id, userId, request), Message = "Updated." });
        }

        [HttpDelete("{id:guid}/soft-delete")]
        [Authorize]
        [SwaggerOperation(Summary = "Soft-delete an annotation")]
        public async Task<IActionResult> SoftDelete(Guid manuscriptId, Guid id)
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
