using System.Globalization;

namespace MangaManagementSystem.Business.Services.Implements.Ranking
{
    internal static class RankingPeriodHelper
    {
        public static string NormalizeMonthlyPeriod(string? period)
        {
            if (string.IsNullOrWhiteSpace(period))
                throw new ArgumentException("Period is required.");

            if (!DateOnly.TryParseExact(
                    period.Trim(),
                    RankingConstants.PeriodFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                throw new ArgumentException($"Period must use format {RankingConstants.PeriodFormat}.");
            }

            if (parsed.Day != RankingConstants.MonthlyPeriodDay)
                throw new ArgumentException("Ranking period must use day 01 for monthly ranking.");

            return parsed.ToString(RankingConstants.PeriodFormat, CultureInfo.InvariantCulture);
        }
    }
}
