using MangaManagementSystem.DataAccess.Entities.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Repositories.Interfaces
{
    /// <summary>
    /// Repository chuyên biệt cho Manuscript với các query phức tạp.
    /// Extends IRepository&lt;Manuscript&gt; cho CRUD cơ bản.
    /// </summary>
    public interface IManuscriptRepository : IRepository<Manuscript>
    {
        /// <summary>
        /// Lấy tất cả manuscripts của một chapter, kèm Chapter → Series để mapping DTO đầy đủ.
        /// Sắp xếp theo VersionNo tăng dần.
        /// </summary>
        Task<List<Manuscript>> GetByChapterIdAsync(Guid chapterId, CancellationToken ct = default);

        /// <summary>
        /// Lấy toàn bộ manuscripts trong hệ thống kèm Chapter → Series.
        /// Dùng cho endpoint GET /manuscripts (danh sách tổng).
        /// </summary>
        Task<List<Manuscript>> GetAllWithDetailsAsync(CancellationToken ct = default);

        /// <summary>
        /// Tính % tiến độ hoàn thành chapter = (PageTask Approved / Tổng PageTask) * 100.
        /// Trả về 0 nếu chapter chưa có PageTask nào.
        /// </summary>
        Task<int> GetChapterProgressAsync(Guid chapterId, CancellationToken ct = default);

        /// <summary>
        /// Lấy manuscript có VersionNo cao nhất (latest version) của chapter.
        /// Trả null nếu chapter chưa có manuscript nào.
        /// </summary>
        Task<Manuscript?> GetLatestByChapterIdAsync(Guid chapterId, CancellationToken ct = default);

        /// <summary>
        /// Lấy manuscript theo ID kèm Chapter → Series để check authorization (BR-74).
        /// </summary>
        Task<Manuscript?> GetByIdWithDetailsAsync(Guid manuscriptId, CancellationToken ct = default);

        /// <summary>
        /// Tính VersionNo tiếp theo cho chapter (Max hiện tại + 1, hoặc 1 nếu chưa có).
        /// Dùng khi Submit manuscript mới (BR-73).
        /// </summary>
        Task<int> GetNextVersionNoAsync(Guid chapterId, CancellationToken ct = default);

        /// <summary>
        /// Kiểm tra chapter đã có manuscript nào đang ở trạng thái Approved không (BR-80).
        /// Dùng trước khi cho phép Submit.
        /// </summary>
        Task<bool> HasApprovedManuscriptAsync(Guid chapterId, CancellationToken ct = default);

        /// <summary>
        /// Lấy Chapter và Series tương ứng theo ChapterId.
        /// </summary>
        Task<Chapter?> GetChapterWithSeriesAsync(Guid chapterId, CancellationToken ct = default);

        /// <summary>
        /// Đếm tổng số PageTasks thuộc Chapter.
        /// </summary>
        Task<int> GetTotalPageTasksCountAsync(Guid chapterId, CancellationToken ct = default);

        /// <summary>
        /// Đếm số PageTasks chưa được duyệt (Status != approvedStatus) thuộc Chapter.
        /// </summary>
        Task<int> GetUnapprovedPageTasksCountAsync(Guid chapterId, string approvedStatus, CancellationToken ct = default);

        /// <summary>
        /// Đếm số PageTasks đã được duyệt (Status == approvedStatus) thuộc Chapter.
        /// </summary>
        Task<int> GetApprovedPageTasksCountAsync(Guid chapterId, string approvedStatus, CancellationToken ct = default);

        /// <summary>
        /// Lấy Role Name của User theo UserId.
        /// </summary>
        Task<string?> GetUserRoleNameAsync(Guid userId, CancellationToken ct = default);
    }
}
