using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.DTOs.Requests.Auth
{
    public class RegisterRequest
    {
        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string Password { get; set; } = null!;

        public Guid RoleId { get; set; }

        public Guid? AssignedFromUserId { get; set; }
    }
}
