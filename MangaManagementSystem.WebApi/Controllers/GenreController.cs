using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/genres")]
    [Produces("application/json")]
    [Tags("Genres")]
    public class GenreController : ControllerBase
    {
        private readonly IGenreService _genreService;
        public GenreController(IGenreService genreService) => _genreService = genreService;

        [HttpGet]
        [Authorize]
        [SwaggerOperation(Summary = "Get all genres")]
        public async Task<IActionResult> GetAll()
            => Ok(new BaseResponse { Data = await _genreService.GetAllAsync(), Message = "Success" });

        [HttpGet("{id:guid}")]
        [Authorize]
        [SwaggerOperation(Summary = "Get genre by ID")]
        public async Task<IActionResult> GetById(Guid id)
            => Ok(new BaseResponse { Data = await _genreService.GetByIdAsync(id), Message = "Success" });

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Create a genre")]
        public async Task<IActionResult> Create([FromBody] GenreRequest request)
        {
            var result = await _genreService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.GenreId }, new BaseResponse { Data = result, Message = "Genre created." });
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Update a genre")]
        public async Task<IActionResult> Update(Guid id, [FromBody] GenreRequest request)
            => Ok(new BaseResponse { Data = await _genreService.UpdateAsync(id, request), Message = "Updated." });

        [HttpDelete("{id:guid}/soft-delete")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Soft-delete a genre")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _genreService.SoftDeleteAsync(id);
            return Ok(new BaseResponse { Message = "Genre deleted." });
        }
    }
}
