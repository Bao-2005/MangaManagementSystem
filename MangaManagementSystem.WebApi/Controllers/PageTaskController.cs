using Microsoft.AspNetCore.Mvc;
using MangaManagementSystem.Business.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WarehouseService.Application.DTOs;
using MangaManagementSystem.Business.DTOs.Requests.Tasks;
using MangaManagementSystem.Business.Services.Interfaces.Tasks;

namespace MangaManagementSystem.API.Controllers;

    [ApiController]
[Route("api/page-tasks")]
    [Produces("application/json")]
[Tags("Page Tasks")]
    public class PageTaskController : ControllerBase
    {
    private readonly IPageTaskService _pageTaskService;

    public PageTaskController(IPageTaskService pageTaskService)
    {
        _pageTaskService = pageTaskService;
    }

    [HttpPost]
    [Authorize(Policy = "MangakaOnly")]
    [SwaggerOperation(
        Summary = "Assign page task to assistant",
        Description = "Mangaka-only. Creates a page task for an assistant on a chapter/manuscript that belongs to the authenticated mangaka.")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreatePageTaskRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

        var task = await _pageTaskService.CreateAsync(userId.Value, request);
        return Ok(new BaseResponse { Data = task, Message = "Task assigned successfully." });
    }

    [HttpGet("mangaka")]
    [Authorize(Policy = "MangakaOnly")]
    [SwaggerOperation(
        Summary = "Get tasks assigned by current mangaka",
        Description = "Mangaka-only. Returns page tasks for chapters in the authenticated mangaka's series.")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMangakaTasks()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

        var tasks = await _pageTaskService.GetMangakaTasksAsync(userId.Value);
        return Ok(new BaseResponse { Data = tasks, Message = "Success" });
    }

    [HttpGet("assistant")]
        [Authorize(Policy = "AssistantOnly")]
    [SwaggerOperation(
        Summary = "Get tasks assigned to current assistant",
        Description = "Assistant-only. Returns page tasks assigned to the authenticated assistant.")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssistantTasks()
        {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

        var tasks = await _pageTaskService.GetAssistantTasksAsync(userId.Value);
        return Ok(new BaseResponse { Data = tasks, Message = "Success" });
        }

    [HttpPost("{pageTaskId:guid}/submissions")]
    [Authorize(Policy = "AssistantOnly")]
    [SwaggerOperation(
        Summary = "Submit completed page task work",
        Description = "Assistant-only. Submits a file asset for the authenticated assistant's assigned page task.")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Submit(Guid pageTaskId, [FromBody] SubmitPageTaskRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

        var task = await _pageTaskService.SubmitAsync(userId.Value, pageTaskId, request);
        return Ok(new BaseResponse { Data = task, Message = "Task submitted successfully." });
    }

    [HttpPost("submissions/{submissionId:guid}/approve")]
        [Authorize(Policy = "MangakaOnly")]
    [SwaggerOperation(
        Summary = "Approve assistant submission",
        Description = "Mangaka-only. Approves a submitted page task submission for the authenticated mangaka's series.")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(Guid submissionId)
        {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

        var task = await _pageTaskService.ApproveSubmissionAsync(userId.Value, submissionId);
        return Ok(new BaseResponse { Data = task, Message = "Submission approved successfully." });
        }

    [HttpPost("submissions/{submissionId:guid}/reject")]
        [Authorize(Policy = "MangakaOnly")]
    [SwaggerOperation(
        Summary = "Reject assistant submission",
        Description = "Mangaka-only. Rejects a submitted page task submission and returns the task to InProgress.")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(Guid submissionId, [FromBody] ReviewPageTaskSubmissionRequest request)
        {
        var userId = GetUserId();
        if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

        var task = await _pageTaskService.RejectSubmissionAsync(userId.Value, submissionId, request);
        return Ok(new BaseResponse { Data = task, Message = "Submission rejected successfully." });
        }

        private Guid? GetUserId()
        {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out var id) ? id : null;
    }
}

