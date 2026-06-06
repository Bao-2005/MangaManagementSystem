using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces
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
