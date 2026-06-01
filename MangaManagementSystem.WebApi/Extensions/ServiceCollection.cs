using MangaManagementSystem.Business.Mappers;
using MangaManagementSystem.Business.Services.Implements;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.DataAccess.Repositories.Implements;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;

namespace MangaManagementSystem.API.Extensions
{
    public static class ServiceCollection
    {
        public static void Register(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPageTaskService, PageTaskService>();
            services.RegisterInfrastructure();
        }
    }
}
