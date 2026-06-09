using MangaManagementSystem.Business.DTOs.Requests.Users;
using MangaManagementSystem.Business.DTOs.Responses.Users;

namespace MangaManagementSystem.Business.Services.Interfaces.Users
{
    public interface IUserAssignmentService
    {
        Task<IEnumerable<UserAssignmentResponse>> GetByMangakaAsync(Guid fromUserId);
        Task<IEnumerable<UserAssignmentResponse>> GetByAssistantAsync(Guid toUserId);
        Task<UserAssignmentResponse> CreateAsync(Guid fromUserId, CreateUserAssignmentRequest request);
        Task UnassignAsync(Guid assignmentId);
        Task SoftDeleteAsync(Guid assignmentId);
    }
}
