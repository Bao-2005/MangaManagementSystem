using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.DTOs.Requests.Users
{
    public class UpdateMyAvatarRequest
    {
        public string OriginalFileName { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public long Length { get; set; }
        public Stream Content { get; set; } = null!;
    }
}
