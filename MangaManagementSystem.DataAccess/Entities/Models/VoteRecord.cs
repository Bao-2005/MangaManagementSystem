using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class VoteRecord
    {
        public Guid VoteRecordId { get; set; }
        public Guid SeriesId { get; set; }

        public string Period { get; set; } = null!;
        public int ReaderCount { get; set; }
        public int VoteCount { get; set; }
        public string Status { get; set; } = null!;
        public Guid? ConfirmedBy { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Series Series { get; set; } = null!;
        public User? Confirmer { get; set; }
    }
}
