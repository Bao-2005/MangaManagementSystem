using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/page-tasks")]
    [Produces("application/json")]
    [Tags("Page Tasks")]
    public class PageTasksController : ControllerBase
    {
        private readonly IPageTaskService _pageTaskService;

        public PageTasksController(IPageTaskService pageTaskService)
        {
            _pageTaskService = pageTaskService;
        }

        [HttpPost]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(
            Summary = "Create page task",
            Description = "Mangaka assigns a page task to an Assistant currently assigned to them.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreatePageTaskRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            var task = await _pageTaskService.CreateAsync(userId.Value, request);

            return Ok(new BaseResponse { Data = task, Message = "Success" });
        }

        [HttpGet("mangaka")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(
            Summary = "Get Mangaka page tasks",
            Description = "Returns page tasks for chapters owned by the authenticated Mangaka.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetForMangaka()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            var tasks = await _pageTaskService.GetForMangakaAsync(userId.Value);

            return Ok(new BaseResponse { Data = tasks, Message = "Success" });
        }

        [HttpGet("assistant")]
        [Authorize(Policy = "AssistantOnly")]
        [SwaggerOperation(
            Summary = "Get Assistant page tasks",
            Description = "Returns page tasks assigned to the authenticated Assistant.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetForAssistant()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            var tasks = await _pageTaskService.GetForAssistantAsync(userId.Value);

            return Ok(new BaseResponse { Data = tasks, Message = "Success" });
        }

        [HttpPost("{pageTaskId:guid}/submissions")]
        [Authorize(Policy = "AssistantOnly")]
        [SwaggerOperation(
            Summary = "Submit page task",
            Description = "Assistant submits a file asset for an assigned page task. Rejected tasks can be submitted again as a new version.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Submit(Guid pageTaskId, [FromBody] SubmitPageTaskRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            PageTaskSubmissionResponse submission = await _pageTaskService.SubmitAsync(userId.Value, pageTaskId, request);

            return Ok(new BaseResponse { Data = submission, Message = "Success" });
        }

        [HttpPost("submissions/{submissionId:guid}/approve")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(
            Summary = "Approve page task submission",
            Description = "Mangaka approves a submitted task version for a chapter they own.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Approve(Guid submissionId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            var submission = await _pageTaskService.ApproveSubmissionAsync(userId.Value, submissionId);

            return Ok(new BaseResponse { Data = submission, Message = "Success" });
        }

        [HttpPost("submissions/{submissionId:guid}/reject")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(
            Summary = "Reject page task submission",
            Description = "Mangaka rejects a submitted task version and records the rejection reason.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Reject(Guid submissionId, [FromBody] RejectPageTaskSubmissionRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            var submission = await _pageTaskService.RejectSubmissionAsync(userId.Value, submissionId, request);

            return Ok(new BaseResponse { Data = submission, Message = "Success" });
        }

        private Guid? GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }
    }
}
