using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.DTOs.Responses.Auth
{
    public class RoleResponse
    {
        public Guid RoleId { get; set; }

        public string RoleName { get; set; } = null!;
    }
}
