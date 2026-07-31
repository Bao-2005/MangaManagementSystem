using MangaManagementSystem.Business.DTOs.Requests.Ranking;
using MangaManagementSystem.Business.Services.Interfaces.Ranking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/ranking")]
    [Produces("application/json")]
    [Tags("Ranking Workflow")]
    public class RankingWorkflowController : ControllerBase
    {
        private readonly IRankingWorkflowService _rankingWorkflowService;

        public RankingWorkflowController(IRankingWorkflowService rankingWorkflowService)
        {
            _rankingWorkflowService = rankingWorkflowService;
        }

        [HttpPost("vote-records")]
        [Authorize(Policy = "EditorialBoardOnly")]
        [SwaggerOperation(
            Summary = "Create a ranking vote record",
            Description = "Editorial Board only. Creates a pending reader vote record for an active series and monthly ranking period.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateVoteRecord([FromBody] CreateRankingVoteRecordRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            var voteRecord = await _rankingWorkflowService.CreateVoteRecordAsync(userId.Value, request);
            return Ok(new BaseResponse { Data = voteRecord, Message = "Vote record created." });
        }

        [HttpPost("vote-records/{id:guid}/confirm")]
        [Authorize(Policy = "EditorialBoardOnly")]
        [SwaggerOperation(
            Summary = "Confirm a ranking vote record",
            Description = "Editorial Board only. Confirms a pending vote record and recalculates all rankings for the same period.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ConfirmVoteRecord(Guid id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            var ranking = await _rankingWorkflowService.ConfirmVoteRecordAsync(userId.Value, id);
            return Ok(new BaseResponse { Data = ranking, Message = "Vote record confirmed and rankings recalculated." });
        }

        [HttpPost("snapshots/{rankingSnapshotId:guid}/elimination-decision")]
        [Authorize(Policy = "EditorialBoardOnly")]
        [SwaggerOperation(
            Summary = "Create an elimination board decision for a low-ranked series",
            Description = "Editorial Board only. Creates an open board decision for a ranking snapshot with score below 20.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateEliminationDecision(
            Guid rankingSnapshotId,
            [FromBody] CreateRankingEliminationDecisionRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized(new BaseResponse { Message = "Unauthorized" });

            var decision = await _rankingWorkflowService.CreateEliminationDecisionAsync(
                userId.Value,
                rankingSnapshotId,
                request);

            return Ok(new BaseResponse { Data = decision, Message = "Elimination decision created." });
        }

        [HttpGet("periods")]
        [Authorize]
        [SwaggerOperation(
            Summary = "Get rankings for a period",
            Description = "Returns recalculated ranking snapshots for a monthly period in dd/MM/yyyy format, using day 01.")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRankings([FromQuery] string period)
        {
            var rankings = await _rankingWorkflowService.GetRankingsAsync(period);
            return Ok(new BaseResponse { Data = rankings, Message = "Success" });
        }

        [HttpGet("series/{seriesId:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get ranking history for a series")]
        [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSeriesRankingHistory(Guid seriesId)
        {
            var rankings = await _rankingWorkflowService.GetSeriesRankingHistoryAsync(seriesId);
            return Ok(new BaseResponse { Data = rankings, Message = "Success" });
        }

        private Guid? GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out var id) ? id : null;
        }
    }
}
