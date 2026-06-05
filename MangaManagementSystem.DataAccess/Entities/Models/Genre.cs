using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class Genre
    {
        public Guid GenreId { get; set; }
        public string Title { get; set; } = null!;
        public DateTime? DeletedAt { get; set; }

        public ICollection<SeriesGenre> SeriesGenres { get; set; } = new List<SeriesGenre>();
    }
}
