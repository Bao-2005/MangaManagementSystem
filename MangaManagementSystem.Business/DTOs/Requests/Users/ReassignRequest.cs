using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.DTOs.Requests.Users
{
    public class ReassignRequest
    {
        [Required]
        public Guid AssignmentId { get; set; }
        [Required]
        public Guid MangakaId { get; set; }
        public Guid FromUserId { get; set; }
    }
}
