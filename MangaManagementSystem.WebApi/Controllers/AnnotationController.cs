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
        [Authorize]
        [SwaggerOperation(Summary = "Add annotation to a manuscript page")]
        public async Task<IActionResult> Create(Guid manuscriptId, [FromBody] CreateAnnotationRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _service.CreateAsync(userId, manuscriptId, request);
            return CreatedAtAction(nameof(GetById), new { manuscriptId, id = result.AnnotationId }, new BaseResponse { Data = result, Message = "Annotation added." });
        }

        [HttpGet("/api/submissions/{submissionId:guid}/annotations")]
        [Authorize]
        [SwaggerOperation(Summary = "Get all annotations for a page task submission")]
        public async Task<IActionResult> GetBySubmission(Guid submissionId)
            => Ok(new BaseResponse { Data = await _service.GetBySubmissionAsync(submissionId), Message = "Success" });

        [HttpPost("/api/submissions/{submissionId:guid}/annotations")]
        [Authorize(Policy = "AssistantOnly")]
        [SwaggerOperation(Summary = "Add annotation to the assistant's own submission")]
        public async Task<IActionResult> CreateForSubmission(
            Guid submissionId,
            [FromBody] CreateSubmissionAnnotationRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _service.CreateForSubmissionAsync(userId, submissionId, request);
            return CreatedAtAction(nameof(GetById), new { manuscriptId = result.ManuscriptId, id = result.AnnotationId }, new BaseResponse { Data = result, Message = "Annotation added." });
        }

        [HttpPut("/api/submissions/{submissionId:guid}/annotations/{id:guid}")]
        [Authorize(Policy = "AssistantOnly")]
        [SwaggerOperation(Summary = "Update one of the assistant's own submission annotations")]
        public async Task<IActionResult> UpdateForSubmission(
            Guid submissionId,
            Guid id,
            [FromBody] UpdateAnnotationRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse { Data = await _service.UpdateForSubmissionAsync(submissionId, id, userId, request), Message = "Updated." });
        }

        [HttpDelete("/api/submissions/{submissionId:guid}/annotations/{id:guid}/soft-delete")]
        [Authorize(Policy = "AssistantOnly")]
        [SwaggerOperation(Summary = "Soft-delete one of the assistant's own submission annotations")]
        public async Task<IActionResult> SoftDeleteForSubmission(Guid submissionId, Guid id)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            await _service.SoftDeleteForSubmissionAsync(submissionId, id, userId);
            return Ok(new BaseResponse { Message = "Deleted." });
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
