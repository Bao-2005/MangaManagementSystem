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
    [Tags("VoteRecords")]
    public class VoteRecordController : ControllerBase
    {
        private readonly IVoteRecordService _service;
        public VoteRecordController(IVoteRecordService service) => _service = service;

        [HttpGet("api/series/{seriesId:guid}/vote-records")]
        [Authorize]
        [SwaggerOperation(Summary = "Get vote records for a series")]
        public async Task<IActionResult> GetBySeries(Guid seriesId)
            => Ok(new BaseResponse { Data = await _service.GetBySeriesAsync(seriesId), Message = "Success" });

        [HttpPost("api/vote-records")]
        [Authorize(Policy = "EditorialBoardOnly")]
        [SwaggerOperation(Summary = "Create a vote record for a series (EditorialBoard only)")]
        public async Task<IActionResult> Create([FromBody] CreateVoteRecordRequest request)
        {
            var result = await _service.CreateAsync(request);
            return Ok(new BaseResponse { Data = result, Message = "Vote record created." });
        }

        [HttpPut("api/vote-records/{id:guid}/confirm")]
        [Authorize(Policy = "EditorialBoardOnly")]
        [SwaggerOperation(Summary = "Confirm a vote record (EditorialBoard only)")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            var userId = GetUserId() ?? throw new UnauthorizedAccessException();
            var result = await _service.ConfirmAsync(id, userId);
            return Ok(new BaseResponse { Data = result, Message = "Vote record confirmed." });
        }

        [HttpDelete("api/vote-records/{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Soft-delete a vote record")]
        public async Task<IActionResult> SoftDelete(Guid id)
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
