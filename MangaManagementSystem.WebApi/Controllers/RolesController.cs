using MangaManagementSystem.Business.DTOs.Requests.Auth;
using MangaManagementSystem.Business.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WarehouseService.Application.DTOs;

namespace MangaManagementSystem.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Tags("Roles")]
    [Authorize(Policy = "AdminOnly")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all roles",
            Description = "Get all available roles in the system.")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _roleService.GetAllAsync();

            return Ok(new BaseResponse
            {
                Data = result,
                Message = "Success"
            });
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get role by ID",
            Description = "Get role information by role ID.")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _roleService.GetByIdAsync(id);

            return Ok(new BaseResponse
            {
                Data = result,
                Message = "Success"
            });
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a role",
            Description = "Create a new role in the system.")]
        public async Task<IActionResult> Create(
            [FromBody] RoleDto request)
        {
            var result = await _roleService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.RoleId },
                new BaseResponse
                {
                    Data = result,
                    Message = "Role created."
                });
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update a role",
            Description = "Update the role name by role ID.")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] RoleDto request)
        {
            var result = await _roleService.UpdateAsync(id, request);

            return Ok(new BaseResponse
            {
                Data = result,
                Message = "Role updated."
            });
        }

        [HttpDelete("{id:guid}/soft-delete")]
        [SwaggerOperation(
            Summary = "Soft-delete a role",
            Description = "Soft-delete a role by setting DeletedAt.")]
        public async Task<IActionResult> SoftDelete(Guid id)
        {
            await _roleService.SoftDeleteAsync(id);

            return Ok(new BaseResponse
            {
                Message = "Role deleted."
            });
        }
    }
}
