using MangaManagementSystem.Business.DTOs.Requests.Users;
using MangaManagementSystem.Business.DTOs.Responses.Users;
using MangaManagementSystem.Business.Services.Interfaces.Users;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Users
{
    public class UserAssignmentService : IUserAssignmentService
    {
        private readonly IRepository<UserAssignment> _repo;
        private readonly IRepository<User> _userRepository;

        public UserAssignmentService(IRepository<UserAssignment> repo,
            IRepository<User> userRepository) 
        {
            _repo = repo;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserAssignmentResponse>> GetByMangakaAsync(Guid mangakaId)
            => await _repo.GetAll().Include(a => a.FromUser).Include(a => a.ToUser)
                .Where(a => a.ToUserId == mangakaId && a.DeletedAt == null)
                .Select(a => Map(a)).ToListAsync();

        public async Task<IEnumerable<UserAssignmentResponse>> GetByTantouEditorAsync(Guid tantouEditorId)
            => await _repo.GetAll().Include(a => a.FromUser).Include(a => a.ToUser)
                .Where(a => a.FromUserId == tantouEditorId && a.DeletedAt == null)
                .Select(a => Map(a)).ToListAsync();

        public async Task<UserAssignmentResponse> CreateAsync(Guid fromUserId, CreateUserAssignmentRequest request)
        {
            var assignment = new UserAssignment
            {
                FromUserId = fromUserId,
                ToUserId = request.ToUserId,
                AssignedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(assignment);
            await _repo.SaveChangeAsync();
            return await _repo.GetAll().Include(a => a.FromUser).Include(a => a.ToUser)
                .Where(a => a.AssignmentId == assignment.AssignmentId)
                .Select(a => Map(a)).FirstAsync();
        }

        public async Task<List<UserAssignmentResponse>> GetByUserIdAsync(Guid userId)
        {
            var assignment = await _repo.GetAll().Include(x => x.FromUser).Include(x => x.ToUser).Where(a => a.FromUserId == userId || a.ToUserId == userId).ToListAsync() ?? throw new KeyNotFoundException("No assignment found for this user");
            return assignment.Select(x => Map(x)).ToList();
        }

        public async Task UnassignAsync(Guid assignmentId)
        {
            var a = await _repo.GetAll().FirstOrDefaultAsync(x => x.AssignmentId == assignmentId && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Assignment not found.");
            a.UnassignedAt = DateTime.UtcNow;
            _repo.Update(a);
            await _repo.SaveChangeAsync();
        }

        public async Task ReassignUserAsync(ReassignRequest request)
        {
            //var user = await _repo.GetAll().FirstOrDefaultAsync(x => x.ToUserId == request.MangakaId && x.FromUserId == request.FromUserId && x.DeletedAt == null)
            //        ?? throw new KeyNotFoundException("Không tồn tại quan hệ này");
            var fromUser = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.UserId == request.FromUserId) ?? throw new KeyNotFoundException("Người dùng không tồn tại");
            var mangaka = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.UserId == request.MangakaId) ?? throw new KeyNotFoundException("Người dùng không tồn tại");
            var assignment = await _repo.GetAll().FirstOrDefaultAsync(x => x.AssignmentId == request.AssignmentId && x.DeletedAt == null) ?? throw new KeyNotFoundException("Không tồn tại quan hệ này");
            assignment.UnassignedAt = DateTime.UtcNow;
            assignment.DeletedAt = DateTime.UtcNow;
            var newAssignment = new UserAssignment()
            {
                FromUserId = request.FromUserId,
                ToUserId = request.MangakaId,
                AssignedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(newAssignment);
            await _repo.SaveChangeAsync();
        }

        public async Task SoftDeleteAsync(Guid assignmentId)
        {
            var a = await _repo.GetAll().FirstOrDefaultAsync(x => x.AssignmentId == assignmentId && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Assignment not found.");
            a.DeletedAt = DateTime.UtcNow;
            _repo.Update(a);
            await _repo.SaveChangeAsync();
        }

        private static UserAssignmentResponse Map(UserAssignment a) => new()
        {
            AssignmentId = a.AssignmentId, FromUserId = a.FromUserId, FromUserName = a.FromUser?.DisplayName ?? "",
            ToUserId = a.ToUserId, ToUserName = a.ToUser?.DisplayName ?? "",
            ToUserEmail = a.ToUser?.Email ?? "",
            AssignedAt = a.AssignedAt, UnassignedAt = a.UnassignedAt
        };
    }
}
