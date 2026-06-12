using MangaManagementSystem.Business.DTOs.Responses.Users;
using MangaManagementSystem.Business.Services.Interfaces.Users;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
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

        public UserService(
            IRepository<User> userRepository,
            IRepository<UserAssignment> userAssignmentRepository,
            IRepository<PageTask> pageTaskRepository,
            IRepository<Annotation> annotationRepository,
            IRepository<MangaManagementSystem.DataAccess.Entities.Models.Series> seriesRepository)
        {
            _userRepository = userRepository;
            _userAssignmentRepository = userAssignmentRepository;
            _pageTaskRepository = pageTaskRepository;
            _annotationRepository = annotationRepository;
            _seriesRepository = seriesRepository;
        }

        public async Task<IEnumerable<UserProfileResponse>> GetAllAsync()
        {
            return await _userRepository.GetAll()
                .Include(x => x.Role)
                .Include(x => x.AssignmentsToUser)
                    .ThenInclude(x => x.FromUser)
                .Where(x => x.DeletedAt == null)
                .Select(x => new
                {
                    User = x,
                    AssignedEditor = x.AssignmentsToUser
                        .Where(a => a.DeletedAt == null && a.UnassignedAt == null && a.FromUser.DeletedAt == null)
                        .OrderByDescending(a => a.AssignedAt)
                        .Select(a => new
                        {
                            a.FromUserId,
                            a.FromUser.DisplayName
                        })
                        .FirstOrDefault()
                })
                .Select(x => new UserProfileResponse
                {
                    UserId = x.User.UserId,
                    UserName = x.User.UserName,
                    Email = x.User.Email,
                    DisplayName = x.User.DisplayName,
                    RoleName = x.User.Role.RoleName,
                    AssignedEditorId = x.AssignedEditor == null ? null : x.AssignedEditor.FromUserId,
                    AssignedEditorName = x.AssignedEditor == null ? null : x.AssignedEditor.DisplayName,
                    CreatedAt = x.User.CreatedAt,
                    LastLoginAt = x.User.LastLoginAt,
                    DeletedAt = x.User.DeletedAt
                })
                .ToListAsync();
        }

        public async Task SoftDeleteAsync(Guid userId)
        {
            var user = await _userRepository.GetAll()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.DeletedAt == null);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var now = DateTime.UtcNow;
            user.DeletedAt = now;

            // Cascade: UserAssignments (both directions)
            var assignments = await _userAssignmentRepository.GetAll()
                .Where(x => (x.FromUserId == userId || x.ToUserId == userId) && x.DeletedAt == null)
                .ToListAsync();
            foreach (var a in assignments)
                a.DeletedAt = now;

            // Cascade: PageTasks assigned to this user
            var pageTasks = await _pageTaskRepository.GetAll()
                .Where(x => x.AssistantId == userId && x.DeletedAt == null)
                .ToListAsync();
            foreach (var pt in pageTasks)
                pt.DeletedAt = now;

            // Cascade: Annotations authored by this user
            var annotations = await _annotationRepository.GetAll()
                .Where(x => x.AuthorId == userId && x.DeletedAt == null)
                .ToListAsync();
            foreach (var a in annotations)
                a.DeletedAt = now;

            // Note: FileAssets have no direct user FK on the entity - no cascade needed here

            // Cascade: Series owned by this Mangaka
            var series = await _seriesRepository.GetAll()
                .Where(x => x.MangakaId == userId && x.DeletedAt == null)
                .ToListAsync();
            foreach (var s in series)
                s.DeletedAt = now;

            await _userRepository.SaveChangeAsync();
        }

        public async Task<IEnumerable<UserProfileResponse>> GetAssignedMangakasAsync(Guid editorId)
        {
            return await _userRepository.GetAll()
                .Include(x => x.Role)
                .Include(x => x.AssignmentsToUser)
                    .ThenInclude(x => x.FromUser)
                .Where(x => x.DeletedAt == null
                    && x.AssignmentsToUser.Any(a =>
                        a.FromUserId == editorId
                        && a.DeletedAt == null
                        && a.UnassignedAt == null))
                .Select(x => new
                {
                    User = x,
                    AssignedEditor = x.AssignmentsToUser
                        .Where(a => a.DeletedAt == null && a.UnassignedAt == null && a.FromUser.DeletedAt == null)
                        .OrderByDescending(a => a.AssignedAt)
                        .Select(a => new { a.FromUserId, a.FromUser.DisplayName })
                        .FirstOrDefault()
                })
                .Select(x => new UserProfileResponse
                {
                    UserId = x.User.UserId,
                    UserName = x.User.UserName,
                    Email = x.User.Email,
                    DisplayName = x.User.DisplayName,
                    RoleName = x.User.Role.RoleName,
                    AssignedEditorId = x.AssignedEditor == null ? null : x.AssignedEditor.FromUserId,
                    AssignedEditorName = x.AssignedEditor == null ? null : x.AssignedEditor.DisplayName,
                    CreatedAt = x.User.CreatedAt,
                    LastLoginAt = x.User.LastLoginAt,
                    DeletedAt = x.User.DeletedAt
                })
                .ToListAsync();
        }
    }
}
