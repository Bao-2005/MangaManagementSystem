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
    [Produces("application/json")]
    [Tags("BoardDecisions")]
    public class BoardDecisionController : ControllerBase
    {
        private readonly IBoardDecisionService _decisionService;
        private readonly IBoardVoteService _voteService;

        public BoardDecisionController(IBoardDecisionService decisionService, IBoardVoteService voteService)
        {
            _decisionService = decisionService;
            _voteService = voteService;
        }

        [HttpGet("api/series/{seriesId:guid}/board-decisions")]
        [Authorize]
        [SwaggerOperation(Summary = "Get board decisions for a series")]
        public async Task<IActionResult> GetBySeries(Guid seriesId)
            => Ok(new BaseResponse { Data = await _decisionService.GetBySeriesAsync(seriesId), Message = "Success" });

        [HttpGet("api/board-decisions/{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get board decision by ID")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(new BaseResponse { Data = await _decisionService.GetByIdAsync(id), Message = "Success" });

        [HttpPost("api/board-decisions")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Create a board decision")]
        public async Task<IActionResult> Create([FromBody] CreateBoardDecisionRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _decisionService.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetById), new { id = result.BoardDecisionId }, new BaseResponse { Data = result, Message = "Decision created." });
        }

        [HttpPut("api/board-decisions/{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Update a board decision")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBoardDecisionRequest request)
            => Ok(new BaseResponse { Data = await _decisionService.UpdateAsync(id, request), Message = "Updated." });

        [HttpPost("api/board-decisions/{id:guid}/extend-deadline")]
        [Authorize(Policy = "EditorInChiefOnly")]
        [SwaggerOperation(Summary = "Extend a tied or no-quorum board decision deadline")]
        public async Task<IActionResult> ExtendDeadline(Guid id, [FromBody] ExtendBoardDecisionRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse
            {
                Data = await _decisionService.ExtendDeadlineAsync(id, userId, request),
                Message = "Deadline extended."
            });
        }

        [HttpPost("api/board-decisions/{id:guid}/special-decision")]
        [Authorize(Policy = "EditorInChiefOnly")]
        [SwaggerOperation(Summary = "Make a special decision after an extended tied or no-quorum board decision")]
        public async Task<IActionResult> SpecialDecision(Guid id, [FromBody] SpecialBoardDecisionRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse
            {
                Data = await _decisionService.SpecialDecisionAsync(id, userId, request),
                Message = "Special decision recorded."
            });
        }

        [HttpDelete("api/board-decisions/{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Soft-delete a board decision")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _decisionService.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Deleted." });
        }

        // --- Board Vote sub-resource ---

        [HttpGet("api/board-decisions/{boardDecisionId:guid}/votes")]
        [Authorize]
        [SwaggerOperation(Summary = "Get votes for a board decision")]
        public async Task<IActionResult> GetVotes(Guid boardDecisionId)
            => Ok(new BaseResponse { Data = await _voteService.GetByDecisionAsync(boardDecisionId), Message = "Success" });

        [HttpPost("api/board-decisions/{boardDecisionId:guid}/votes")]
        [Authorize(Policy = "EditorialBoardOnly")]
        [SwaggerOperation(Summary = "Cast a board vote")]
        public async Task<IActionResult> CastVote(Guid boardDecisionId, [FromBody] CreateBoardVoteRequest request)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            return Ok(new BaseResponse { Data = await _voteService.CastVoteAsync(userId, boardDecisionId, request), Message = "Vote cast." });
        }

        [HttpDelete("api/board-votes/{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Soft-delete a board vote")]
        public async Task<IActionResult> SoftDeleteVote(Guid id)
        {
            await _voteService.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Vote deleted." });
        }

        private Guid? GetUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out var id) ? id : null;
        }
    }
}
