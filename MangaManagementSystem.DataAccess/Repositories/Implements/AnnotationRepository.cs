using MangaManagement.DataAccess.DbContexts;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Repositories.Implements
{
    public class AnnotationRepository : Repository<Annotation>, IAnnotationRepository
    {
        private readonly MangaDbContext _context;

        public AnnotationRepository(MangaDbContext context) : base(context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<List<Annotation>> GetByManuscriptVersionAsync(
            Guid manuscriptId,
            int versionNo,
            int? pageNo = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Annotations
                .Where(a => a.ManuscriptId == manuscriptId
                         && a.VersionNo == versionNo
                         && !a.IsDeleted);

            if (pageNo.HasValue)
            {
                query = query.Where(a => a.PageNo == pageNo.Value);
            }

            return await query
                .OrderBy(a => a.PageNo)
                .ThenBy(a => a.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> CountByManuscriptVersionAsync(
            Guid manuscriptId,
            int versionNo,
            CancellationToken cancellationToken = default)
        {
            return await _context.Annotations
                .CountAsync(a => a.ManuscriptId == manuscriptId
                              && a.VersionNo == versionNo
                              && !a.IsDeleted,
                            cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Annotation?> GetByIdAsync(
            Guid annotationId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Annotations
                .FirstOrDefaultAsync(a => a.AnnotationId == annotationId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int?> GetLatestManuscriptVersionNoAsync(
            Guid chapterId,
            CancellationToken cancellationToken = default)
        {
            var maxVersion = await _context.Manuscripts
                .Where(m => m.ChapterId == chapterId)
                .MaxAsync(m => (int?)m.VersionNo, cancellationToken);

            return maxVersion;
        }

        /// <inheritdoc />
        public async Task<Manuscript?> GetManuscriptWithDetailsAsync(Guid manuscriptId, CancellationToken cancellationToken = default)
        {
            return await _context.Manuscripts
                .Include(m => m.Chapter)
                    .ThenInclude(c => c.Series)
                .FirstOrDefaultAsync(m => m.ManuscriptId == manuscriptId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<string?> GetUserRoleNameAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => u.Role.RoleName)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
