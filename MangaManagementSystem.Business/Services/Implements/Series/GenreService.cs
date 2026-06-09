using MangaManagementSystem.Business.DTOs.Requests.Series;
using MangaManagementSystem.Business.DTOs.Responses.Series;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.DataAccess.Entities.Models;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.Business.Services.Implements.Series
{
    public class GenreService : IGenreService
    {
        private readonly IRepository<Genre> _genreRepository;

        public GenreService(IRepository<Genre> genreRepository)
        {
            _genreRepository = genreRepository;
        }

        public async Task<IEnumerable<GenreResponse>> GetAllAsync()
        {
            return await _genreRepository.GetAll()
                .Where(g => g.DeletedAt == null)
                .Select(g => new GenreResponse { GenreId = g.GenreId, Title = g.Title })
                .ToListAsync();
        }

        public async Task<GenreResponse> GetByIdAsync(Guid id)
        {
            var genre = await _genreRepository.GetAll()
                .FirstOrDefaultAsync(g => g.GenreId == id && g.DeletedAt == null)
                ?? throw new KeyNotFoundException("Genre not found.");
            return new GenreResponse { GenreId = genre.GenreId, Title = genre.Title };
        }

        public async Task<GenreResponse> CreateAsync(GenreRequest request)
        {
            var genre = new Genre { Title = request.Title };
            await _genreRepository.AddAsync(genre);
            await _genreRepository.SaveChangeAsync();
            return new GenreResponse { GenreId = genre.GenreId, Title = genre.Title };
        }

        public async Task<GenreResponse> UpdateAsync(Guid id, GenreRequest request)
        {
            var genre = await _genreRepository.GetAll()
                .FirstOrDefaultAsync(g => g.GenreId == id && g.DeletedAt == null)
                ?? throw new KeyNotFoundException("Genre not found.");
            genre.Title = request.Title;
            _genreRepository.Update(genre);
            await _genreRepository.SaveChangeAsync();
            return new GenreResponse { GenreId = genre.GenreId, Title = genre.Title };
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var genre = await _genreRepository.GetAll()
                .FirstOrDefaultAsync(g => g.GenreId == id && g.DeletedAt == null)
                ?? throw new KeyNotFoundException("Genre not found.");
            genre.DeletedAt = DateTime.UtcNow;
            _genreRepository.Update(genre);
            await _genreRepository.SaveChangeAsync();
        }
    }
}
