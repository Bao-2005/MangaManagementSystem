using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.DataAccess.Entities.Models
{
    public class User
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string DisplayName { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Status { get; set; } = "Active";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<FileAsset> UploadedFiles { get; set; } = new List<FileAsset>();

        public ICollection<PageTask> AssignedPageTasks { get; set; } = new List<PageTask>();

        public ICollection<Annotation> Annotations { get; set; } = new List<Annotation>();

        /// <summary>Các bản thảo mà user này đã submit (với tư cách Mangaka).</summary>
        public ICollection<Manuscript> SubmittedManuscripts { get; set; } = new List<Manuscript>();

        /// <summary>Các bản thảo mà user này đã review (với tư cách Tantou Editor).</summary>
        public ICollection<Manuscript> ReviewedManuscripts { get; set; } = new List<Manuscript>();
    }
}
