using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.DTOs.Requests.Auth
{
    public class RoleDto
    {
        [Required(ErrorMessage = "Role name is required.")]
        [MaxLength(50, ErrorMessage = "Role name must not exceed 50 characters.")]
        public string RoleName { get; set; } = null!;
    }
}
