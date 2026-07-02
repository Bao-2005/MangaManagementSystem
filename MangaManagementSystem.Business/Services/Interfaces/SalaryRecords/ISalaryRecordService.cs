using MangaManagementSystem.Business.DTOs.Responses.SalaryRecords;

namespace MangaManagementSystem.Business.Services.Interfaces.SalaryRecords
{
    public interface ISalaryRecordService
    {
        Task<IEnumerable<SalaryRecordResponse>> GetAsync(
            Guid requesterId,
            string requesterRole,
            Guid? assistantId);
    }
}
