using MangaManagementSystem.Business.Auth.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace MangaManagementSystem.WebApi.Services
{
    public class JwtCurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtCurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? GetCurrentUserId()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                // Fallback to "sub" claim if NameIdentifier is not present (standard JWT)
                userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            }
            
            return Guid.TryParse(userIdStr, out var id) ? id : null;
        }

        public bool BypassAuthorization => false;
    }
}
