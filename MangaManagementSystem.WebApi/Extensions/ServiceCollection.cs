using MangaManagementSystem.Business.Annotations.Interfaces;
using MangaManagementSystem.Business.Annotations.Services;
using MangaManagementSystem.Business.Mappers;
using MangaManagementSystem.DataAccess.Repositories.Implements;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;

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

            services.RegisterInfrastructure();
        }
    }
}

