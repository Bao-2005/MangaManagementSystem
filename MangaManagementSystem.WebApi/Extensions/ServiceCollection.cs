using MangaManagementSystem.Business.Auth.Interfaces;
using MangaManagementSystem.Business.Mappers;
using MangaManagementSystem.Business.Services.Implements;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.DataAccess.Repositories.Implements;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using MangaManagementSystem.WebApi.Services;

namespace MangaManagementSystem.API.Extensions
{
    public static class ServiceCollection
    {
        public static void Register(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Annotation feature
            services.AddScoped<IAnnotationRepository, AnnotationRepository>();
            services.AddScoped<IAnnotationService, AnnotationService>();

            // Manuscript feature
            services.AddScoped<IManuscriptRepository, ManuscriptRepository>();
            services.AddScoped<IManuscriptService, ManuscriptService>();

            // Auth / Current user
            // DEV MODE: bỏ qua toàn bộ authorize khi test
            // TODO (teammate): đổi thành JwtCurrentUserService sau khi implement JWT
            services.AddScoped<ICurrentUserService, DevCurrentUserService>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.RegisterInfrastructure();
        }
    }
}

