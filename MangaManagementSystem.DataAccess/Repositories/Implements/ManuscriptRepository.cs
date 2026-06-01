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
    public class ManuscriptRepository : Repository<Manuscript>, IManuscriptRepository
    {
        private readonly MangaDbContext _context;

        public ManuscriptRepository(MangaDbContext context) : base(context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<List<Manuscript>> GetByChapterIdAsync(
            Guid chapterId,
            CancellationToken ct = default)
        {
            return await _context.Manuscripts
                .Where(m => m.ChapterId == chapterId)
                .OrderBy(m => m.VersionNo)
                .ToListAsync(ct);
        }

        /// <inheritdoc />
        public async Task<Manuscript?> GetLatestByChapterIdAsync(
            Guid chapterId,
            CancellationToken ct = default)
        {
            return await _context.Manuscripts
                .Where(m => m.ChapterId == chapterId)
                .OrderByDescending(m => m.VersionNo)
                .FirstOrDefaultAsync(ct);
        }

        /// <inheritdoc />
        public async Task<Manuscript?> GetByIdWithDetailsAsync(
            Guid manuscriptId,
            CancellationToken ct = default)
        {
            return await _context.Manuscripts
                .Include(m => m.Chapter)
                    .ThenInclude(c => c.Series)
                .FirstOrDefaultAsync(m => m.ManuscriptId == manuscriptId, ct);
        }

        /// <inheritdoc />
        public async Task<int> GetNextVersionNoAsync(
            Guid chapterId,
            CancellationToken ct = default)
        {
            var maxVersion = await _context.Manuscripts
                .Where(m => m.ChapterId == chapterId)
                .MaxAsync(m => (int?)m.VersionNo, ct);

            return (maxVersion ?? 0) + 1;
        }

        /// <inheritdoc />
        public async Task<bool> HasApprovedManuscriptAsync(
            Guid chapterId,
            CancellationToken ct = default)
        {
            return await _context.Manuscripts
                .AnyAsync(m => m.ChapterId == chapterId
                            && m.Status == "Approved", ct);
        }
    }
}
