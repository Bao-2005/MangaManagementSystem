using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/series/{seriesId:guid}/proposal-pages")]
    [Produces("application/json")]
    [Tags("ProposalPages")]
    public class ProposalPageController : ControllerBase
    {
        private readonly IProposalPageService _service;
        public ProposalPageController(IProposalPageService service) => _service = service;

        [HttpGet]
        [Authorize]
        [SwaggerOperation(Summary = "Get all proposal pages for a series")]
        public async Task<IActionResult> GetBySeries(Guid seriesId)
            => Ok(new BaseResponse { Data = await _service.GetBySeriesAsync(seriesId), Message = "Success" });

        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get a proposal page by ID")]
        public async Task<IActionResult> GetById(Guid seriesId, Guid id)
            => Ok(new BaseResponse { Data = await _service.GetByIdAsync(id), Message = "Success" });

        [HttpPost]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Add a proposal page to a series")]
        public async Task<IActionResult> Create(Guid seriesId, [FromBody] CreateProposalPageRequest request)
        {
            var result = await _service.CreateAsync(seriesId, request);
            return CreatedAtAction(nameof(GetById), new { seriesId, id = result.ProposalPageId }, new BaseResponse { Data = result, Message = "Created." });
        }

        [HttpDelete("{id:guid}/soft-delete")]
        [Authorize(Policy = "MangakaOnly")]
        [SwaggerOperation(Summary = "Soft-delete a proposal page")]
        public async Task<IActionResult> SoftDelete(Guid seriesId, Guid id)
        {
            await _service.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Deleted." });
        }
    }
}
