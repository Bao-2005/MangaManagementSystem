using MangaManagement.DataAccess.DbContexts;
using MangaManagementSystem.Business.Services.Interfaces.Series;
using Microsoft.EntityFrameworkCore;

namespace MangaManagementSystem.API.BackgroundServices
{
    public class BoardDecisionDeadlineWorker : BackgroundService
    {
        private const string OpenStatus = "Open";
        private const int DefaultIntervalMinutes = 1;
        private const int MinimumIntervalMinutes = 1;
        private const int MaximumIntervalMinutes = 5;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BoardDecisionDeadlineWorker> _logger;
        private readonly TimeSpan _interval;

        public BoardDecisionDeadlineWorker(
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration,
            ILogger<BoardDecisionDeadlineWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            var configuredMinutes = configuration.GetValue<int?>("BoardDecisionDeadlineWorker:IntervalMinutes")
                ?? DefaultIntervalMinutes;
            var intervalMinutes = Math.Clamp(configuredMinutes, MinimumIntervalMinutes, MaximumIntervalMinutes);
            _interval = TimeSpan.FromMinutes(intervalMinutes);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Board decision deadline worker started with interval {IntervalMinutes} minute(s).",
                _interval.TotalMinutes);

            await ProcessExpiredDecisionsAsync(stoppingToken);

            using var timer = new PeriodicTimer(_interval);
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ProcessExpiredDecisionsAsync(stoppingToken);
            }
        }

        private async Task ProcessExpiredDecisionsAsync(CancellationToken stoppingToken)
        {
            var expiredDecisionIds = new List<Guid>();

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<MangaDbContext>();
                var now = DateTime.UtcNow;

                expiredDecisionIds = await dbContext.BoardDecisions
                    .AsNoTracking()
                    .Where(d => d.DeletedAt == null
                        && d.FinalizedAt == null
                        && d.Status == OpenStatus
                        && d.VotingDeadline <= now)
                    .Select(d => d.BoardDecisionId)
                    .ToListAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Board decision deadline worker batch failed.");
            }

            foreach (var decisionId in expiredDecisionIds)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var finalizationService = scope.ServiceProvider.GetRequiredService<IBoardDecisionFinalizationService>();
                    await finalizationService.ProcessDeadlineAsync(decisionId);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to process board decision deadline for {BoardDecisionId}.",
                        decisionId);
                }
            }
        }
    }
}
