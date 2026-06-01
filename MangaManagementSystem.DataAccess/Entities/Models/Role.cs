using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class Role
    {
        public Guid RoleId { get; set; }

        public string RoleName { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();

        //public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
