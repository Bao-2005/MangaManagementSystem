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

            // Auth & User
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            // Domain services
            services.AddScoped<IGenreService, GenreService>();
            services.AddScoped<ISeriesService, SeriesService>();
            services.AddScoped<IProposalPageService, ProposalPageService>();
            services.AddScoped<IChapterService, ChapterService>();
            services.AddScoped<IManuscriptService, ManuscriptService>();
            services.AddScoped<IChapterPageService, ChapterPageService>();
            services.AddScoped<IPageTaskService, PageTaskService>();
            services.AddScoped<IPageTaskSubmissionService, PageTaskSubmissionService>();
            services.AddScoped<IAnnotationService, AnnotationService>();
            services.AddScoped<IBoardDecisionService, BoardDecisionService>();
            services.AddScoped<IBoardVoteService, BoardVoteService>();
            services.AddScoped<IVoteRecordService, VoteRecordService>();
            services.AddScoped<IRankingSnapshotService, RankingSnapshotService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IUserAssignmentService, UserAssignmentService>();
            services.AddScoped<IEscalationService, EscalationService>();

            services.RegisterInfrastructure();
        }
    }
}
