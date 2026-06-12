using MangaManagementSystem.DataAccess.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaManagementSystem.Business.Services.Interfaces.Auth
{
    public interface IJwtTokenService
    {
        string GenerateAccessToken(User user);

        string GenerateRefreshToken();

        string HashToken(string token);
    }
}
