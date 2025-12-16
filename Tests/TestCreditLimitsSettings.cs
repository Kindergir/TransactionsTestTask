using JFF;

namespace Tests;

// must be from app.settings, test environment
internal class TestCreditLimitsSettings : ICreditLimitsSettings
{
    private ulong _dailyLimit = 5000;
    private ulong _weekyLimit = 20000;
    private ulong _maxDailyLoadCount = 3;
    private DayOfWeek _firstDayOfWeek = DayOfWeek.Monday;
    private ulong _maxDailyLimitForPrimeId = 9999;
    private bool _useRulesForPrimeId;
    private ulong _dailyCountInBucksForPrimeId = 1;
    private IList<DayOfWeek> _suspuciousDaysForDouble = new List<DayOfWeek>();
    private bool _useSuspuciousDaysForDouble = false;

    public ulong DailyLimitInBucks
    {
        get => _dailyLimit;
        set => _dailyLimit = value;
    }

    public ulong WeeklyLimitInBucks
    {
        get => _weekyLimit;
        set => _weekyLimit = value;
    }

    public ulong MaxDailyLoadCount
    {
        get => _maxDailyLoadCount;
        set => _maxDailyLoadCount = value;
    }

    public ulong DaylyLimitInBucksForPrimeId
    {
        get => _maxDailyLimitForPrimeId;
        set => _maxDailyLimitForPrimeId = value;
    }

    public ulong DailyCountForPrimeId
    {
        get => _dailyCountInBucksForPrimeId;
        set => _dailyCountInBucksForPrimeId = value;
    }

    public bool UseRulesForPrimeId
    {
        get => _useRulesForPrimeId;
        set => _useRulesForPrimeId = value;
    }

    public DayOfWeek FirstDayOfWeek
    {
        get => _firstDayOfWeek;
        set => _firstDayOfWeek = value;
    }

    public IList<DayOfWeek> SuspuciousDaysForDouble
    {
        get => _suspuciousDaysForDouble;
        set => _suspuciousDaysForDouble = value;
    }

    public bool UseSuspuciousDaysForDouble
    {
        get => _useSuspuciousDaysForDouble;
        set => _useSuspuciousDaysForDouble = value;
    }
}