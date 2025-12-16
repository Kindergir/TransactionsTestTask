namespace JFF;

public interface ICreditLimitsSettings
{
    public ulong DailyLimitInBucks { get; set; }
    public ulong WeeklyLimitInBucks { get; set; }
    public ulong MaxDailyLoadCount { get; set; }
    public ulong DaylyLimitInBucksForPrimeId { get; set; }
    public ulong DailyCountForPrimeId { get; set; }
    public bool UseRulesForPrimeId { get; set; }
    public DayOfWeek FirstDayOfWeek { get; set; }
    public IList<DayOfWeek> SuspuciousDaysForDouble { get; set; }
    public bool UseSuspuciousDaysForDouble { get; set; }
}