using MangaManagementSystem.Business.DTOs.Requests.Users;
using MangaManagementSystem.Business.DTOs.Responses.Users;

namespace MangaManagementSystem.Business.Services.Interfaces.Users
{
    public interface IUserAssignmentService
    {
        Task<IEnumerable<UserAssignmentResponse>> GetByMangakaAsync(Guid mangakaId);
        Task<IEnumerable<UserAssignmentResponse>> GetByTantouEditorAsync(Guid tantouEditorId);
        Task<UserAssignmentResponse> CreateAsync(Guid fromUserId, CreateUserAssignmentRequest request);
        Task UnassignAsync(Guid assignmentId);
        Task SoftDeleteAsync(Guid assignmentId);
    }
}
