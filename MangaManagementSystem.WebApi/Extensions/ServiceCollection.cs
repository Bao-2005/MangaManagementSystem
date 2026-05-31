using MangaManagementSystem.Business.Annotations.Interfaces;
using MangaManagementSystem.Business.Annotations.Services;
using MangaManagementSystem.Business.Auth.Interfaces;
using MangaManagementSystem.Business.Mappers;
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

            // Auth / Current user
            // DEV MODE: bỏ qua toàn bộ authorize khi test
            // TODO (teammate): đổi thành JwtCurrentUserService sau khi implement JWT
            services.AddScoped<ICurrentUserService, DevCurrentUserService>();

            services.RegisterInfrastructure();
        }
    }
}

