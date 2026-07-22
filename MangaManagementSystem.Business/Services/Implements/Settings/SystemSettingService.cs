using MangaManagementSystem.Business.DTOs.Responses.Settings;
using MangaManagementSystem.Business.Services.Interfaces.Settings;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Settings
{
    public class SystemSettingService : ISystemSettingService
    {
        public const string MaxSubmissionAttemptsKey = "PageTask.MaxSubmissionAttempts";
        private const int DefaultMaxSubmissionAttempts = 3;
        private const int MinMaxSubmissionAttempts = 1;
        private const int MaxMaxSubmissionAttempts = 20;
        private const string MaxSubmissionAttemptsDescription =
            "Maximum number of submission attempts allowed for a page task.";

        private readonly IRepository<SystemSetting> _systemSettingRepository;

        public SystemSettingService(IRepository<SystemSetting> systemSettingRepository)
        {
            _systemSettingRepository = systemSettingRepository;
        }

        public async Task<int> GetMaxSubmissionAttemptsValueAsync()
        {
            var setting = await GetActiveSettingAsync(MaxSubmissionAttemptsKey);
            return setting == null
                ? DefaultMaxSubmissionAttempts
                : ParseMaxSubmissionAttempts(setting.Value);
        }

        public async Task<SystemSettingResponse> GetMaxSubmissionAttemptsAsync()
        {
            var setting = await GetActiveSettingAsync(MaxSubmissionAttemptsKey);
            if (setting == null)
            {
                return new SystemSettingResponse
                {
                    Key = MaxSubmissionAttemptsKey,
                    Value = DefaultMaxSubmissionAttempts,
                    Description = MaxSubmissionAttemptsDescription
                };
            }

            return new SystemSettingResponse
            {
                Key = setting.Key,
                Value = ParseMaxSubmissionAttempts(setting.Value),
                Description = setting.Description,
                UpdatedAt = setting.UpdatedAt
            };
        }

        public async Task<SystemSettingResponse> UpsertMaxSubmissionAttemptsAsync(int value)
        {
            ValidateMaxSubmissionAttempts(value);

            var now = DateTime.UtcNow;
            var setting = await GetActiveSettingAsync(MaxSubmissionAttemptsKey);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = MaxSubmissionAttemptsKey,
                    Value = value.ToString(),
                    Description = MaxSubmissionAttemptsDescription,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _systemSettingRepository.AddAsync(setting);
            }
            else
            {
                setting.Value = value.ToString();
                setting.Description = string.IsNullOrWhiteSpace(setting.Description)
                    ? MaxSubmissionAttemptsDescription
                    : setting.Description;
                setting.UpdatedAt = now;
                _systemSettingRepository.Update(setting);
            }

            await _systemSettingRepository.SaveChangeAsync();

            return new SystemSettingResponse
            {
                Key = setting.Key,
                Value = value,
                Description = setting.Description,
                UpdatedAt = setting.UpdatedAt
            };
        }

        private Task<SystemSetting?> GetActiveSettingAsync(string key)
        {
            return _systemSettingRepository.GetAll()
                .FirstOrDefaultAsync(x => x.Key == key && x.DeletedAt == null);
        }

        private static int ParseMaxSubmissionAttempts(string value)
        {
            if (!int.TryParse(value, out var parsed))
                throw new InvalidOperationException("Max submission attempts setting must be an integer.");

            ValidateMaxSubmissionAttempts(parsed);
            return parsed;
        }

        private static void ValidateMaxSubmissionAttempts(int value)
        {
            if (value is < MinMaxSubmissionAttempts or > MaxMaxSubmissionAttempts)
                throw new ArgumentException("Max submission attempts must be between 1 and 20.");
        }
    }
}
