using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace MangaManagementSystem.Business.DTOs.Requests.Tasks
{
    public class CreateSubmissionAnnotationRequest
    {
        [Required]
        public int PageNo { get; set; }

        [Required]
        [Range(0, 1)]
        public decimal PositionX { get; set; }

        [Required]
        [Range(0, 1)]
        public decimal PositionY { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = null!;
    }
}

