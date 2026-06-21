using MangaManagementSystem.Business.DTOs.Requests.Users;
using MangaManagementSystem.Business.DTOs.Responses.Users;
using MangaManagementSystem.Business.Services.Interfaces.Users;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserAssignment> _userAssignmentRepository;
        private readonly IRepository<PageTask> _pageTaskRepository;
        private readonly IRepository<Annotation> _annotationRepository;
        private readonly IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> _seriesRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(
            IRepository<User> userRepository,
            IRepository<UserAssignment> userAssignmentRepository,
            IRepository<PageTask> pageTaskRepository,
            IRepository<Annotation> annotationRepository,
            IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> seriesRepository,
            IRepository<Role> roleRepository)
        {
            _userRepository = userRepository;
            _userAssignmentRepository = userAssignmentRepository;
            _pageTaskRepository = pageTaskRepository;
            _annotationRepository = annotationRepository;
            _seriesRepository = seriesRepository;
            _roleRepository = roleRepository;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<IEnumerable<UserProfileResponse>> GetAllAsync()
        {
            return await UserProfileQuery()
                .Where(x => x.User.DeletedAt == null)
                .Select(x => MapProfile(x.User, x.AssignedEditor))
                .ToListAsync();
        }

        public async Task<UserProfileResponse> AdminUpdateAsync(Guid userId, AdminUpdateUserRequest request)
        {
            var user = await _userRepository.GetAll()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("User not found.");

            if (request.UserName != null)
            {
                var userName = RequireText(request.UserName, "UserName");
                var exists = await _userRepository.GetAll()
                    .AnyAsync(x => x.UserId != userId
                        && x.DeletedAt == null
                        && x.UserName == userName);
                if (exists)
                    throw new InvalidOperationException("Username already exists.");

                user.UserName = userName;
            }

            if (request.Email != null)
            {
                var email = RequireText(request.Email, "Email").ToLowerInvariant();
                var exists = await _userRepository.GetAll()
                    .AnyAsync(x => x.UserId != userId
                        && x.DeletedAt == null
                        && x.Email.ToLower() == email);
                if (exists)
                    throw new InvalidOperationException("Email already exists.");

                user.Email = email;
            }

            if (request.DisplayName != null)
                user.DisplayName = RequireText(request.DisplayName, "DisplayName");

            if (request.RoleId.HasValue)
            {
                if (request.RoleId.Value == Guid.Empty)
                    throw new ArgumentException("RoleId is invalid.");

                var roleExists = await _roleRepository.GetAll()
                    .AnyAsync(x => x.RoleId == request.RoleId.Value && x.DeletedAt == null);
                if (!roleExists)
                    throw new KeyNotFoundException("Role not found.");

                user.RoleId = request.RoleId.Value;
            }

            if (request.NewPassword != null)
            {
                var newPassword = RequireText(request.NewPassword, "NewPassword");
                if (newPassword.Length < 6)
                    throw new ArgumentException("NewPassword must be at least 6 characters.");

                user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
                user.RefreshTokenHash = null;
                user.RefreshTokenExpiresAt = null;
            }

            await _userRepository.SaveChangeAsync();
            return await GetByIdForAdminAsync(userId);
        }

        public async Task<UserProfileResponse> UpdateMyProfileAsync(Guid userId, UpdateMyProfileRequest request)
        {
            var user = await _userRepository.GetAll()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.DeletedAt == null)
                ?? throw new KeyNotFoundException("User not found.");

            if (request.UserName != null)
            {
                var userName = RequireText(request.UserName, "UserName");
                var exists = await _userRepository.GetAll()
                    .AnyAsync(x => x.UserId != userId
                        && x.DeletedAt == null
                        && x.UserName == userName);
                if (exists)
                    throw new InvalidOperationException("Username already exists.");

                user.UserName = userName;
            }

            if (request.Email != null)
            {
                var email = RequireText(request.Email, "Email").ToLowerInvariant();
                var exists = await _userRepository.GetAll()
                    .AnyAsync(x => x.UserId != userId
                        && x.DeletedAt == null
                        && x.Email.ToLower() == email);
                if (exists)
                    throw new InvalidOperationException("Email already exists.");

                user.Email = email;
            }

            if (request.DisplayName != null)
                user.DisplayName = RequireText(request.DisplayName, "DisplayName");

            await _userRepository.SaveChangeAsync();
            return await GetByIdForAdminAsync(userId);
        }

        public async Task SoftDeleteAsync(Guid userId)
        {
            var user = await _userRepository.GetAll()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.DeletedAt == null);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var now = DateTime.UtcNow;
            user.DeletedAt = now;

            var assignments = await _userAssignmentRepository.GetAll()
                .Where(x => (x.FromUserId == userId || x.ToUserId == userId) && x.DeletedAt == null)
                .ToListAsync();
            foreach (var a in assignments)
                a.DeletedAt = now;

            var pageTasks = await _pageTaskRepository.GetAll()
                .Where(x => x.AssistantId == userId && x.DeletedAt == null)
                .ToListAsync();
            foreach (var pt in pageTasks)
                pt.DeletedAt = now;

            var annotations = await _annotationRepository.GetAll()
                .Where(x => x.AuthorId == userId && x.DeletedAt == null)
                .ToListAsync();
            foreach (var a in annotations)
                a.DeletedAt = now;

            var series = await _seriesRepository.GetAll()
                .Where(x => x.MangakaId == userId && x.DeletedAt == null)
                .ToListAsync();
            foreach (var s in series)
                s.DeletedAt = now;

            await _userRepository.SaveChangeAsync();
        }

        public async Task<IEnumerable<UserProfileResponse>> GetAssignedMangakasAsync(Guid editorId)
        {
            return await UserProfileQuery()
                .Where(x => x.User.DeletedAt == null
                    && x.User.AssignmentsToUser.Any(a =>
                        a.FromUserId == editorId
                        && a.DeletedAt == null
                        && a.UnassignedAt == null))
                .Select(x => MapProfile(x.User, x.AssignedEditor))
                .ToListAsync();
        }

        private async Task<UserProfileResponse> GetByIdForAdminAsync(Guid userId)
        {
            return await UserProfileQuery()
                .Where(x => x.User.UserId == userId && x.User.DeletedAt == null)
                .Select(x => MapProfile(x.User, x.AssignedEditor))
                .FirstAsync();
        }

        private IQueryable<UserProfileProjection> UserProfileQuery()
        {
            return _userRepository.GetAll()
                .Include(x => x.Role)
                .Include(x => x.AssignmentsToUser)
                    .ThenInclude(x => x.FromUser)
                .Select(x => new UserProfileProjection
                {
                    User = x,
                    AssignedEditor = x.AssignmentsToUser
                        .Where(a => a.DeletedAt == null
                            && a.UnassignedAt == null
                            && a.FromUser.DeletedAt == null)
                        .OrderByDescending(a => a.AssignedAt)
                        .Select(a => new AssignedEditorProjection
                        {
                            FromUserId = a.FromUserId,
                            DisplayName = a.FromUser.DisplayName
                        })
                        .FirstOrDefault()
                });
        }

        private static UserProfileResponse MapProfile(
            User user,
            AssignedEditorProjection? assignedEditor)
        {
            return new UserProfileResponse
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                RoleName = user.Role.RoleName,
                AssignedEditorId = assignedEditor == null ? null : assignedEditor.FromUserId,
                AssignedEditorName = assignedEditor == null ? null : assignedEditor.DisplayName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                DeletedAt = user.DeletedAt
            };
        }

        private static string RequireText(string? value, string fieldName)
        {
            var normalized = value?.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                throw new ArgumentException($"{fieldName} is required.");
            return normalized;
        }

        private sealed class UserProfileProjection
        {
            public User User { get; set; } = null!;
            public AssignedEditorProjection? AssignedEditor { get; set; }
        }

        private sealed class AssignedEditorProjection
        {
            public Guid FromUserId { get; set; }
            public string DisplayName { get; set; } = null!;
        }
    }
}
