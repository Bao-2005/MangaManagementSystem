using MangaManagementSystem.Business.DTOs.Requests.Users;
using MangaManagementSystem.Business.DTOs.Responses.Users;

namespace MangaManagementSystem.Business.Services.Interfaces.Users
{
    public interface IUserService
    {
        Task<IEnumerable<UserProfileResponse>> GetAllAsync();
        Task<IEnumerable<UserProfileResponse>> GetAssistantsAsync();
        Task<UserProfileResponse> AdminUpdateAsync(Guid userId, AdminUpdateUserRequest request);
        Task<UserProfileResponse> UpdateMyProfileAsync(Guid userId, UpdateMyProfileRequest request);
        Task SoftDeleteAsync(Guid userId);
        Task<IEnumerable<UserProfileResponse>> GetAssignedMangakasAsync(Guid editorId);
        Task<UserProfileResponse> UpdateMyAvatarAsync(Guid userId, UpdateMyAvatarRequest request, CancellationToken cancellationToken = default);
    }
}
