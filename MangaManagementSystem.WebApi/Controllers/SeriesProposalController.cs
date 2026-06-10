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
    [Route("api/proposals")]
    [Produces("application/json")]
    [Tags("Proposals")]
    public class SeriesProposalController : ControllerBase
    {
        private readonly ISeriesProposalWorkflowService _workflowService;

        public SeriesProposalController(ISeriesProposalWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpPost("{seriesId:guid}/submit-review")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Submit a draft proposal for Tantou review")]
        public async Task<IActionResult> SubmitReview(Guid seriesId)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _workflowService.SubmitForReviewAsync(seriesId, userId);
            return Ok(new BaseResponse { Data = result, Message = "Proposal submitted for review." });
        }

        [HttpPost("{seriesId:guid}/reject")]
        [Authorize(Policy = "TantouEditorOnly")]
        [SwaggerOperation(Summary = "Reject an under-review proposal as assigned Tantou Editor")]
        public async Task<IActionResult> Reject(Guid seriesId, [FromBody] RejectProposalRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _workflowService.RejectAsync(seriesId, userId, request);
            return Ok(new BaseResponse { Data = result, Message = "Proposal rejected." });
        }

        [HttpPost("{seriesId:guid}/submit-to-board")]
        [HttpPost("{seriesId:guid}/submit-board")]
        [Authorize(Policy = "TantouEditorOnly")]
        [SwaggerOperation(Summary = "Submit an under-review proposal to the editorial board")]
        public async Task<IActionResult> SubmitToBoard(Guid seriesId)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _workflowService.SubmitToBoardAsync(seriesId, userId);
            return Ok(new BaseResponse { Data = result, Message = "Proposal submitted to editorial board." });
        }

        [HttpPost("{seriesId:guid}/activate")]
        [Authorize(Policy = "TantouEditorOnly")]
        [SwaggerOperation(Summary = "Activate an approved proposal as assigned Tantou Editor")]
        public async Task<IActionResult> Activate(Guid seriesId)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _workflowService.ActivateAsync(seriesId, userId);
            return Ok(new BaseResponse { Data = result, Message = "Series activated." });
        }

        private Guid? GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out var id) ? id : null;
        }
    }
}
