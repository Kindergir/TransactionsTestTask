using JFF.DB;
using JFF.DTO;

namespace JFF.Logic;

public class TransactionsValidator(ITransactionsRepository transactionsRepository, ICreditLimitsSettings settings)
{
    public bool Validate(Transaction transaction)
    {
        if (transaction.Amount.Value < 0 || transaction.Amount.Currency == Currency.Unknown)
            return false;

        var dayStart = transaction.Timestamp.Date;
        var dayEnd = dayStart.AddDays(1);

        var transactionAmount = settings.UseSuspuciousDaysForDouble
            ? transaction.Amount.Value * 2
            : transaction.Amount.Value;

        var idCanBeParsed = ulong.TryParse(transaction.Id, out var parsedId);
        var infoByDay = transactionsRepository.GetSumAndCountByPeriod(transaction.CustomerId, new Period(dayStart, dayEnd));
        if (idCanBeParsed && settings.UseRulesForPrimeId && parsedId.IsPrime())
        {
            if (transactionAmount + infoByDay.Sum > settings.DaylyLimitInBucksForPrimeId
                || infoByDay.Count >= settings.DailyCountForPrimeId)
                return false;
        }
        else
        {
            if (infoByDay.Sum + transactionAmount > settings.DailyLimitInBucks
                || infoByDay.Count >= settings.MaxDailyLoadCount)
                return false;
        }

        var weekStart = GetWeekStart(transaction.Timestamp, settings);
        var weekEnd = weekStart.AddDays(7);

        var infoByWeek = transactionsRepository.GetSumAndCountByPeriod(transaction.CustomerId, new Period(weekStart, weekEnd));
        if (infoByWeek.Sum + transactionAmount > settings.WeeklyLimitInBucks)
            return false;

        return true;
    }

    private static DateTime GetWeekStart(DateTimeOffset date, ICreditLimitsSettings settings)
    {
        var diff = (7 + (date.DayOfWeek - settings.FirstDayOfWeek)) % 7;
        return date.Date.AddDays(-diff);
    }
}