using MangaManagementSystem.Business.DTOs.Responses.Users;

namespace MangaManagementSystem.Business.Services.Interfaces.Users
{
    public interface IUserService
    {
        Task<IEnumerable<UserProfileResponse>> GetAllAsync();
        Task SoftDeleteAsync(Guid userId);
    }
}
