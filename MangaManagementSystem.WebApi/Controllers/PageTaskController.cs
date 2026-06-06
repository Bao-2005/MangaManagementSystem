using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Tags("PageTasks")]
    public class PageTaskController : ControllerBase
    {
        private readonly IPageTaskService _service;
        public PageTaskController(IPageTaskService service) => _service = service;

        [HttpGet("api/chapters/{chapterId:guid}/tasks")]
        [Authorize]
        [SwaggerOperation(Summary = "Get page tasks by chapter")]
        public async Task<IActionResult> GetByChapter(Guid chapterId)
            => Ok(new BaseResponse { Data = await _service.GetByChapterAsync(chapterId), Message = "Success" });

        [HttpGet("api/manuscripts/{manuscriptId:guid}/tasks")]
        [Authorize]
        [SwaggerOperation(Summary = "Get page tasks by manuscript")]
        public async Task<IActionResult> GetByManuscript(Guid manuscriptId)
            => Ok(new BaseResponse { Data = await _service.GetByManuscriptAsync(manuscriptId), Message = "Success" });

        [HttpGet("api/tasks/my")]
        [Authorize(Policy = "AssistantOnly")]
        [SwaggerOperation(Summary = "Get my assigned tasks (Assistant only)")]
        public async Task<IActionResult> GetMy()
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse { Data = await _service.GetByAssistantAsync(userId), Message = "Success" });
        }

        [HttpGet("api/tasks/{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get page task by ID")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(new BaseResponse { Data = await _service.GetByIdAsync(id), Message = "Success" });

        [HttpPost("api/tasks")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Create and assign a page task")]
        public async Task<IActionResult> Create([FromBody] CreatePageTaskRequest request)
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.PageTaskId }, new BaseResponse { Data = result, Message = "Task created." });
        }

        [HttpPut("api/tasks/{id:guid}")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Update a page task")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePageTaskRequest request)
            => Ok(new BaseResponse { Data = await _service.UpdateAsync(id, request), Message = "Updated." });

        [HttpDelete("api/tasks/{id:guid}/soft-delete")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Soft-delete a page task")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _service.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Task deleted." });
        }

        private Guid? GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out var id) ? id : null;
        }
    }
}
