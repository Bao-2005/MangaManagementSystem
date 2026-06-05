using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class SeriesGenre
    {
        public Guid SeriesGenreId { get; set; }
        public Guid SeriesId { get; set; }
        public Guid GenreId { get; set; }

        public Series Series { get; set; } = null!;
        public Genre Genre { get; set; } = null!;
    }
}
