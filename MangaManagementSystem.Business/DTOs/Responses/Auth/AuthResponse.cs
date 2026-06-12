using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.DTOs.Responses.Auth
{
    public class AuthResponse
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string RoleName { get; set; } = null!;
    }
}
