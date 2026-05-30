using MangaManagementSystem.DataAccess.Entities.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Repositories.Interfaces
{
    /// <summary>
    /// Repository chuyên biệt cho Annotation với các query phức tạp.
    /// Extends IRepository<Annotation> cho CRUD cơ bản.
    /// </summary>
    public interface IAnnotationRepository : IRepository<Annotation>
    {
        /// <summary>
        /// Lấy danh sách annotation không bị xóa theo manuscriptId và versionNo.
        /// Nếu pageNo có giá trị, filter thêm theo pageNo.
        /// </summary>
        Task<List<Annotation>> GetByManuscriptVersionAsync(
            Guid manuscriptId,
            int versionNo,
            int? pageNo = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Đếm annotation không bị xóa theo manuscriptId và versionNo.
        /// Dùng để enforce BR-77: phải có ít nhất 1 annotation trước khi Revision Required.
        /// </summary>
        Task<int> CountByManuscriptVersionAsync(
            Guid manuscriptId,
            int versionNo,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tìm một annotation theo ID, bao gồm cả đã bị xóa (để audit).
        /// </summary>
        Task<Annotation?> GetByIdAsync(
            Guid annotationId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy VersionNo cao nhất của manuscript theo ChapterId.
        /// Dùng để check BR-75: chỉ annotate trên latest version.
        /// </summary>
        Task<int?> GetLatestManuscriptVersionNoAsync(
            Guid chapterId,
            CancellationToken cancellationToken = default);
    }
}
