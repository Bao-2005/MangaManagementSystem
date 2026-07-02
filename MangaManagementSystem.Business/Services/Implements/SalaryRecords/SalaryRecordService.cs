using MangaManagementSystem.Business.DTOs.Responses.SalaryRecords;
using MangaManagementSystem.Business.Services.Interfaces.SalaryRecords;
using MangaManagementSystem.DataAccess.Entities.Enums;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.SalaryRecords
{
    public class SalaryRecordService : ISalaryRecordService
    {
        private readonly IRepository<SalaryRecord> _salaryRecordRepository;

        public SalaryRecordService(IRepository<SalaryRecord> salaryRecordRepository)
        {
            _salaryRecordRepository = salaryRecordRepository;
        }

        public async Task<IEnumerable<SalaryRecordResponse>> GetAsync(
            Guid requesterId,
            string requesterRole,
            Guid? assistantId)
        {
            var query = _salaryRecordRepository.GetAll()
                .AsNoTracking()
                .Include(x => x.Assistant)
                .Include(x => x.PageTask)
                    .ThenInclude(x => x.Chapter)
                        .ThenInclude(x => x.Series)
                .Where(x => x.DeletedAt == null);

            if (IsRole(requesterRole, UserRole.Admin))
            {
                if (assistantId.HasValue)
                    query = query.Where(x => x.AssistantId == assistantId.Value);
            }
            else if (IsRole(requesterRole, UserRole.Mangaka))
            {
                query = query.Where(x => x.PageTask.Chapter.Series.MangakaId == requesterId);

                if (assistantId.HasValue)
                    query = query.Where(x => x.AssistantId == assistantId.Value);
            }
            else if (IsRole(requesterRole, UserRole.Assistant))
            {
                if (assistantId.HasValue && assistantId.Value != requesterId)
                    throw new UnauthorizedAccessException("You can only view your own salary records.");

                query = query.Where(x => x.AssistantId == requesterId);
            }
            else
            {
                throw new UnauthorizedAccessException("You are not allowed to view salary records.");
            }

            var records = await query
                .OrderByDescending(x => x.ApprovedAt)
                .ToListAsync();

            return records.Select(Map);
        }

        private static bool IsRole(string roleName, UserRole role)
            => string.Equals(roleName, role.ToString(), StringComparison.OrdinalIgnoreCase);

        private static SalaryRecordResponse Map(SalaryRecord record) => new()
        {
            SalaryRecordId = record.SalaryRecordId,
            AssistantId = record.AssistantId,
            AssistantName = record.Assistant?.DisplayName,
            PageTaskId = record.PageTaskId,
            TaskType = record.PageTask?.TaskType,
            PageStart = record.PageTask?.PageStart ?? 0,
            PageEnd = record.PageTask?.PageEnd ?? 0,
            Pages = record.Pages,
            RateAtApproval = record.RateAtApproval,
            Amount = record.Amount,
            ApprovedAt = record.ApprovedAt,
            CreatedAt = record.CreatedAt
        };
    }
}
