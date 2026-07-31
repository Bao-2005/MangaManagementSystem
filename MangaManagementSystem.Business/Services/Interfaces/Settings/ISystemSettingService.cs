using MangaManagementSystem.Business.DTOs.Responses.Settings;

namespace MangaManagementSystem.Business.Services.Interfaces.Settings
{
    public interface ISystemSettingService
    {
        Task<int> GetMaxSubmissionAttemptsValueAsync();

        Task<SystemSettingResponse> GetMaxSubmissionAttemptsAsync();

        Task<SystemSettingResponse> UpsertMaxSubmissionAttemptsAsync(int value);
    }
}
