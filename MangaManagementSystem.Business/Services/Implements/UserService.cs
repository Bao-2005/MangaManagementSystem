using MangaManagementSystem.Business.DTOs.Responses;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserAssignment> _userAssignmentRepository;
        private readonly IRepository<PageTask> _pageTaskRepository;
        private readonly IRepository<Annotation> _annotationRepository;
        private readonly IRepository<FileAsset> _fileAssetRepository;
        private readonly IRepository<Series> _seriesRepository;

        public UserService(
            IRepository<User> userRepository,
            IRepository<UserAssignment> userAssignmentRepository,
            IRepository<PageTask> pageTaskRepository,
            IRepository<Annotation> annotationRepository,
            IRepository<FileAsset> fileAssetRepository,
            IRepository<Series> seriesRepository)
        {
            _userRepository = userRepository;
            _userAssignmentRepository = userAssignmentRepository;
            _pageTaskRepository = pageTaskRepository;
            _annotationRepository = annotationRepository;
            _fileAssetRepository = fileAssetRepository;
            _seriesRepository = seriesRepository;
        }

        public async Task<IEnumerable<UserProfileResponse>> GetAllAsync()
        {
            return await _userRepository.GetAll()
                .Include(x => x.Role)
                .Where(x => x.DeletedAt == null)
                .Select(x => new UserProfileResponse
                {
                    UserId = x.UserId,
                    UserName = x.UserName,
                    Email = x.Email,
                    DisplayName = x.DisplayName,
                    RoleName = x.Role.RoleName,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    LastLoginAt = x.LastLoginAt
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

            // Cascade: FileAssets uploaded by this user
            var fileAssets = await _fileAssetRepository.GetAll()
                .Where(x => x.UploadedBy == userId && x.DeletedAt == null)
                .ToListAsync();
            foreach (var f in fileAssets)
                f.DeletedAt = now;

            // Cascade: Series owned by this Mangaka
            var series = await _seriesRepository.GetAll()
                .Where(x => x.MangakaId == userId && x.DeletedAt == null)
                .ToListAsync();
            foreach (var s in series)
                s.DeletedAt = now;

            await _userRepository.SaveChangeAsync();
        }
    }
}
