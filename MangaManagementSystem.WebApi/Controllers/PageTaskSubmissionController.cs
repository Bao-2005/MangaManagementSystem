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
    [Produces("application/json")]
    [Tags("PageTaskSubmissions")]
    public class PageTaskSubmissionController : ControllerBase
    {
        private readonly IPageTaskSubmissionService _service;
        public PageTaskSubmissionController(IPageTaskSubmissionService service) => _service = service;

        [HttpGet("api/tasks/{pageTaskId:guid}/submissions")]
        [Authorize]
        [SwaggerOperation(Summary = "Get submissions for a page task")]
        public async Task<IActionResult> GetByTask(Guid pageTaskId)
            => Ok(new BaseResponse { Data = await _service.GetByTaskAsync(pageTaskId), Message = "Success" });

        [HttpGet("api/submissions/{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get submission by ID")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(new BaseResponse { Data = await _service.GetByIdAsync(id), Message = "Success" });

        [HttpPost("api/submissions")]
        [Authorize(Policy = "AssistantOnly")]
        [SwaggerOperation(Summary = "Submit work for a page task (Assistant only)")]
        public async Task<IActionResult> Create([FromBody] CreateSubmissionRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _service.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = result.SubmissionId }, new BaseResponse { Data = result, Message = "Submission created." });
        }

        [HttpPut("api/submissions/{id:guid}")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Approve or reject a submission (Mangaka only)")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubmissionRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse { Data = await _service.UpdateAsync(id, request, userId), Message = "Updated." });
        }

        [HttpDelete("api/submissions/{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Soft-delete a submission")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _service.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Submission deleted." });
        }

        private Guid? GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out var id) ? id : null;
        }
    }
}
