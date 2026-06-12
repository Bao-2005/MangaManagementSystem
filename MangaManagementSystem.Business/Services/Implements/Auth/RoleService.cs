using MangaManagementSystem.Business.DTOs.Requests.Auth;
using MangaManagementSystem.Business.DTOs.Responses.Auth;
using MangaManagementSystem.Business.Services.Interfaces.Auth;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.Services.Implements.Auth
{
    public class RoleService : IRoleService
    {
        private const int MaximumRoleNameLength = 50;

        private readonly IRepository<Role> _roleRepository;

        public RoleService(IRepository<Role> roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IEnumerable<RoleResponse>> GetAllAsync()
        {
            return await _roleRepository
                .GetAll()
                .Where(role => role.DeletedAt == null)
                .OrderBy(role => role.RoleName)
                .Select(role => new RoleResponse
                {
                    RoleId = role.RoleId,
                    RoleName = role.RoleName
                })
                .ToListAsync();
        }

        public async Task<RoleResponse> GetByIdAsync(Guid id)
        {
            ValidateId(id);

            var role = await _roleRepository
                .GetAll()
                .FirstOrDefaultAsync(role =>
                    role.RoleId == id &&
                    role.DeletedAt == null)
                ?? throw new KeyNotFoundException("Role not found.");

            return MapToResponse(role);
        }

        public async Task<RoleResponse> CreateAsync(
            RoleDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var roleName = ValidateRoleName(request.RoleName);

            await EnsureRoleNameIsUniqueAsync(roleName);

            var role = new Role
            {
                RoleId = Guid.NewGuid(),
                RoleName = roleName
            };

            await _roleRepository.AddAsync(role);
            await _roleRepository.SaveChangeAsync();

            return MapToResponse(role);
        }

        public async Task<RoleResponse> UpdateAsync(
            Guid id,
            RoleDto request)
        {
            ValidateId(id);
            ArgumentNullException.ThrowIfNull(request);

            var role = await _roleRepository
                .GetAll()
                .FirstOrDefaultAsync(role =>
                    role.RoleId == id &&
                    role.DeletedAt == null)
                ?? throw new KeyNotFoundException("Role not found.");

            var roleName = ValidateRoleName(request.RoleName);

            await EnsureRoleNameIsUniqueAsync(
                roleName,
                role.RoleId);

            role.RoleName = roleName;

            _roleRepository.Update(role);
            await _roleRepository.SaveChangeAsync();

            return MapToResponse(role);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            ValidateId(id);

            var role = await _roleRepository
                .GetAll()
                .FirstOrDefaultAsync(role =>
                    role.RoleId == id &&
                    role.DeletedAt == null)
                ?? throw new KeyNotFoundException("Role not found.");

            role.DeletedAt = DateTime.UtcNow;

            _roleRepository.Update(role);
            await _roleRepository.SaveChangeAsync();
        }

        private async Task EnsureRoleNameIsUniqueAsync(
            string roleName,
            Guid? excludedRoleId = null)
        {
            var normalizedRoleName = roleName.ToLower();

            var roleNameExists = await _roleRepository
                .GetAll()
                .AnyAsync(role =>
                    role.DeletedAt == null &&
                    (!excludedRoleId.HasValue ||
                     role.RoleId != excludedRoleId.Value) &&
                    role.RoleName.ToLower() == normalizedRoleName);

            if (roleNameExists)
            {
                throw new InvalidOperationException(
                    $"Role '{roleName}' already exists.");
            }
        }

        private static string ValidateRoleName(string? roleName)
        {
            var trimmedRoleName = roleName?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedRoleName))
            {
                throw new ArgumentException(
                    "Role name is required.");
            }

            if (trimmedRoleName.Length > MaximumRoleNameLength)
            {
                throw new ArgumentException(
                    $"Role name must not exceed {MaximumRoleNameLength} characters.");
            }

            return trimmedRoleName;
        }

        private static void ValidateId(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException(
                    "Role ID is invalid.");
            }
        }

        private static RoleResponse MapToResponse(Role role)
        {
            return new RoleResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName
            };
        }
    }
}
