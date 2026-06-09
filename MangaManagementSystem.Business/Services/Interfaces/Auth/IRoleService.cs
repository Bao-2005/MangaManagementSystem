using MangaManagementSystem.Business.DTOs.Requests.Auth;
using MangaManagementSystem.Business.DTOs.Responses.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.Services.Interfaces.Auth
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleResponse>> GetAllAsync();

        Task<RoleResponse> GetByIdAsync(Guid id);

        Task<RoleResponse> CreateAsync(RoleDto request);

        Task<RoleResponse> UpdateAsync(
            Guid id,
            RoleDto request);

        Task SoftDeleteAsync(Guid id);
    }
}
