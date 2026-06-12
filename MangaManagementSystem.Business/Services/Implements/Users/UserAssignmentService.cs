using MangaManagementSystem.Business.DTOs.Requests.Users;
using MangaManagementSystem.Business.DTOs.Responses.Users;
using MangaManagementSystem.Business.Services.Interfaces.Users;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Users
{
    public class UserAssignmentService : IUserAssignmentService
    {
        private readonly IRepository<UserAssignment> _repo;

        public UserAssignmentService(IRepository<UserAssignment> repo) => _repo = repo;

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

        public async Task UnassignAsync(Guid assignmentId)
        {
            var a = await _repo.GetAll().FirstOrDefaultAsync(x => x.AssignmentId == assignmentId && x.DeletedAt == null)
                    ?? throw new KeyNotFoundException("Assignment not found.");
            a.UnassignedAt = DateTime.UtcNow;
            _repo.Update(a);
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
