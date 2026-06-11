using MangaManagementSystem.API.BackgroundServices;
using MangaManagementSystem.Business.Mappers;
using MangaManagementSystem.Business.Services.Implements;
using MangaManagementSystem.Business.Services.Implements.Auth;
using MangaManagementSystem.Business.Services.Implements.Chapters;
using MangaManagementSystem.Business.Services.Implements.Files;
using MangaManagementSystem.Business.Services.Implements.Manuscripts;
using MangaManagementSystem.Business.Services.Implements.Series;
using MangaManagementSystem.Business.Services.Implements.Tasks;
using MangaManagementSystem.Business.Services.Implements.Users;
using MangaManagementSystem.Business.Services.Interfaces;
using MangaManagementSystem.Business.Services.Interfaces.Auth;
using MangaManagementSystem.Business.Services.Interfaces.Chapters;
using MangaManagementSystem.Business.Services.Interfaces.Files;
using MangaManagementSystem.Business.Services.Interfaces.Manuscripts;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using MangaManagementSystem.Business.Services.Interfaces.Tasks;
using MangaManagementSystem.Business.Services.Interfaces.Users;
using MangaManagementSystem.DataAccess.Repositories.Implements;
using MangaManagementSystem.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

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
            services.AddScoped<ISeriesProposalWorkflowService, SeriesProposalWorkflowService>();
            services.AddScoped<IProposalPageService, ProposalPageService>();
            services.AddScoped<IChapterService, ChapterService>();
            services.AddScoped<IManuscriptService, ManuscriptService>();
            services.AddScoped<IChapterPageService, ChapterPageService>();
            services.AddScoped<IPageTaskService, PageTaskService>();
            services.AddScoped<IPageTaskSubmissionService, PageTaskSubmissionService>();
            services.AddScoped<IAnnotationService, AnnotationService>();
            services.AddScoped<IBoardDecisionService, BoardDecisionService>();
            services.AddScoped<IBoardDecisionFinalizationService, BoardDecisionFinalizationService>();
            services.AddScoped<IBoardVoteService, BoardVoteService>();
            services.AddScoped<IVoteRecordService, VoteRecordService>();
            services.AddScoped<IRankingSnapshotService, RankingSnapshotService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<INotificationDispatchService, NotificationDispatchService>();
            services.AddScoped<IUserAssignmentService, UserAssignmentService>();
            services.AddScoped<IEscalationService, EscalationService>();
            // Supabase client (singleton — shared, no Realtime)
            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();
                var url = config["Supabase:Url"]!;
                var key = config["Supabase:ServiceRoleKey"]!;
                var options = new Supabase.SupabaseOptions { AutoConnectRealtime = false };
                var client = new Supabase.Client(url, key, options);
                client.InitializeAsync().GetAwaiter().GetResult();
                return client;
            });
            services.AddScoped<IStorageService, SupabaseStorageService>();
            services.AddScoped<IFileUploadService, FileUploadService>();
            services.AddScoped<IRoleService, RoleService>();

            services.AddHostedService<BoardDecisionDeadlineWorker>();

            services.RegisterInfrastructure();
        }
    }
}
