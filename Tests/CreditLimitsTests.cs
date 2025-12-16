using FluentAssertions;
using JFF;
using JFF.DB;
using JFF.DTO;
using JFF.Logic;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Tests;

[TestFixture]
public class CreditLimitsTests
{
    private ServiceProvider _serviceProvider;
    private ITransactionsRepository _transactionsRepository;
    private TransactionsValidator _transactionsValidator;
    private ICreditLimitsSettings _creditLimitsSettings;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITransactionsRepository, FakeTransactionsRepository>();
        services.AddSingleton<TransactionsValidator>();
        services.AddSingleton<ICreditLimitsSettings, TestCreditLimitsSettings>();
        _serviceProvider = services.BuildServiceProvider();
        _transactionsRepository = _serviceProvider.GetService<ITransactionsRepository>();
        _transactionsValidator = _serviceProvider.GetService<TransactionsValidator>();
        _creditLimitsSettings = _serviceProvider.GetService<ICreditLimitsSettings>();
    }

    [Test]
    public void OverLimitPerDay()
    {
        _creditLimitsSettings.UseRulesForPrimeId = false;
        _creditLimitsSettings.UseSuspuciousDaysForDouble = false;

        var customerId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        var thirdLimitPerDay = _creditLimitsSettings.DailyLimitInBucks / 3;
        var remainPerDay = _creditLimitsSettings.DailyLimitInBucks - thirdLimitPerDay * 2;

        CreateTransaction(customerId, thirdLimitPerDay, now);
        CreateTransaction(customerId, thirdLimitPerDay, now);

        _transactionsValidator.Validate(new Transaction
        {
            Id = "3",
            CustomerId = customerId,
            Amount = new LoadAmount(remainPerDay + 1, Currency.USD),
            Timestamp = now
        }).Should().BeFalse();
    }

    [Test]
    public void OverLimitPerSuspiciousDay()
    {
        var customerId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        _creditLimitsSettings.UseRulesForPrimeId = false;
        _creditLimitsSettings.UseSuspuciousDaysForDouble = true;
        _creditLimitsSettings.SuspuciousDaysForDouble = new List<DayOfWeek> { now.DayOfWeek };

        CreateTransaction(customerId, _creditLimitsSettings.DailyLimitInBucks - 1, now);

        _transactionsValidator.Validate(new Transaction
        {
            Id = "3",
            CustomerId = customerId,
            Amount = new LoadAmount(1, Currency.USD),
            Timestamp = now
        }).Should().BeFalse();
    }

    [Test]
    public void ExactlyLimitPerSuspiciousDay()
    {
        var customerId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        _creditLimitsSettings.UseRulesForPrimeId = false;
        _creditLimitsSettings.UseSuspuciousDaysForDouble = true;
        _creditLimitsSettings.SuspuciousDaysForDouble = new List<DayOfWeek> { now.DayOfWeek };

        CreateTransaction(customerId, _creditLimitsSettings.DailyLimitInBucks - 2, now);

        _transactionsValidator.Validate(new Transaction
        {
            Id = "3",
            CustomerId = customerId,
            Amount = new LoadAmount(1, Currency.USD),
            Timestamp = now
        }).Should().BeTrue();
    }

    [Test]
    public void ExactlyLimitPerDay()
    {
        _creditLimitsSettings.UseRulesForPrimeId = false;
        _creditLimitsSettings.UseSuspuciousDaysForDouble = false;

        var customerId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        var thirdLimitPerDay = _creditLimitsSettings.DailyLimitInBucks / 3;
        var remainPerDay = _creditLimitsSettings.DailyLimitInBucks - thirdLimitPerDay * 2;

        CreateTransaction(customerId, thirdLimitPerDay, now);
        CreateTransaction(customerId, thirdLimitPerDay, now);

        _transactionsValidator.Validate(new Transaction
        {
            Id = "3",
            CustomerId = customerId,
            Amount = new LoadAmount(remainPerDay, Currency.USD),
            Timestamp = now
        }).Should().BeTrue();
    }

    [Test]
    public void ExactlyLimitPerDayWithPrimeId()
    {
        _creditLimitsSettings.UseRulesForPrimeId = true;
        _creditLimitsSettings.UseSuspuciousDaysForDouble = false;
        _creditLimitsSettings.DailyCountForPrimeId = 5;

        var customerId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        _transactionsValidator.Validate(new Transaction
        {
            Id = "3",
            CustomerId = customerId,
            Amount = new LoadAmount(_creditLimitsSettings.DaylyLimitInBucksForPrimeId, Currency.USD),
            Timestamp = now
        }).Should().BeTrue();
    }

    [Test]
    public void OverLimitPerDayWithPrimeId()
    {
        _creditLimitsSettings.UseRulesForPrimeId = true;
        _creditLimitsSettings.UseSuspuciousDaysForDouble = false;
        _creditLimitsSettings.DailyCountForPrimeId = 5;

        var customerId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        _transactionsValidator.Validate(new Transaction
        {
            Id = "3",
            CustomerId = customerId,
            Amount = new LoadAmount(_creditLimitsSettings.DaylyLimitInBucksForPrimeId + 1, Currency.USD),
            Timestamp = now
        }).Should().BeFalse();
    }

    [Test]
    public void OverCountPerDayWithPrimeId()
    {
        _creditLimitsSettings.UseRulesForPrimeId = true;
        _creditLimitsSettings.UseSuspuciousDaysForDouble = false;

        var customerId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        CreateTransaction(customerId, 1, now);

        _transactionsValidator.Validate(new Transaction
        {
            Id = "3",
            CustomerId = customerId,
            Amount = new LoadAmount(1, Currency.USD),
            Timestamp = now
        }).Should().BeFalse();
    }

    [Test]
    public void OverCountPerDay()
    {
        _creditLimitsSettings.UseRulesForPrimeId = false;
        _creditLimitsSettings.UseSuspuciousDaysForDouble = false;

        var customerId = Guid.NewGuid().ToString();
        var now = DateTimeOffset.UtcNow;

        var thirdLimitPerDay = _creditLimitsSettings.DailyLimitInBucks / 3;

        CreateTransaction(customerId, thirdLimitPerDay - 1, now);
        CreateTransaction(customerId, thirdLimitPerDay - 1, now);
        CreateTransaction(customerId, thirdLimitPerDay - 1, now);

        _transactionsValidator.Validate(new Transaction
        {
            Id = "3",
            CustomerId = customerId,
            Amount = new LoadAmount(1, Currency.USD),
            Timestamp = now
        }).Should().BeFalse();
    }

    [Test]
    [TestCase(DayOfWeek.Monday)]
    [TestCase(DayOfWeek.Tuesday)]
    [TestCase(DayOfWeek.Wednesday)]
    [TestCase(DayOfWeek.Thursday)]
    [TestCase(DayOfWeek.Friday)]
    [TestCase(DayOfWeek.Saturday)]
    [TestCase(DayOfWeek.Sunday)]
    public void OverLimitPerWeek(DayOfWeek weekStart)
    {
        _creditLimitsSettings.UseRulesForPrimeId = false;
        _creditLimitsSettings.UseSuspuciousDaysForDouble = false;
        _creditLimitsSettings.FirstDayOfWeek = weekStart;

        var customerId = Guid.NewGuid().ToString();

        var today = DateTime.Today;
        var daysUntilNextWeekStart = ((int)weekStart - (int)today.DayOfWeek + 7) % 7;
        var nextWeekStart = today.AddDays(daysUntilNextWeekStart);

        for (int i = 0; i < 6; i++)
        {
            CreateTransaction(customerId,
                _creditLimitsSettings.WeeklyLimitInBucks / 7,
                nextWeekStart + TimeSpan.FromDays(i));
        }

        var remainPerWeek = _creditLimitsSettings.WeeklyLimitInBucks
                            - (_creditLimitsSettings.WeeklyLimitInBucks / 7 * 6);

        _transactionsValidator.Validate(new Transaction
        {
            Id = "3",
            CustomerId = customerId,
            Amount = new LoadAmount(remainPerWeek + 1, Currency.USD),
            Timestamp = nextWeekStart + TimeSpan.FromDays(6)
        }).Should().BeFalse();
    }

    [Test]
    [TestCase(DayOfWeek.Monday)]
    [TestCase(DayOfWeek.Tuesday)]
    [TestCase(DayOfWeek.Wednesday)]
    [TestCase(DayOfWeek.Thursday)]
    [TestCase(DayOfWeek.Friday)]
    [TestCase(DayOfWeek.Saturday)]
    [TestCase(DayOfWeek.Sunday)]
    public void ExactlyLimitPerWeek(DayOfWeek weekStart)
    {
        _creditLimitsSettings.UseRulesForPrimeId = false;
        _creditLimitsSettings.UseSuspuciousDaysForDouble = false;
        _creditLimitsSettings.FirstDayOfWeek = weekStart;

        var customerId = Guid.NewGuid().ToString();

        var today = DateTime.Today;
        var daysUntilNextWeekStart = ((int)weekStart - (int)today.DayOfWeek + 7) % 7;
        var nextWeekStart = today.AddDays(daysUntilNextWeekStart);

        for (int i = 0; i < 6; i++)
        {
            CreateTransaction(customerId,
                _creditLimitsSettings.WeeklyLimitInBucks / 7,
                nextWeekStart + TimeSpan.FromDays(i));
        }

        var remainPerWeek = _creditLimitsSettings.WeeklyLimitInBucks
                            - (_creditLimitsSettings.WeeklyLimitInBucks / 7 * 6);

        _transactionsValidator.Validate(new Transaction
        {
            Id = "3",
            CustomerId = customerId,
            Amount = new LoadAmount(remainPerWeek, Currency.USD),
            Timestamp = nextWeekStart + TimeSpan.FromDays(6)
        }).Should().BeTrue();
    }

    [Test]
    [TestCase(DayOfWeek.Monday)]
    [TestCase(DayOfWeek.Tuesday)]
    [TestCase(DayOfWeek.Wednesday)]
    [TestCase(DayOfWeek.Thursday)]
    [TestCase(DayOfWeek.Friday)]
    [TestCase(DayOfWeek.Saturday)]
    [TestCase(DayOfWeek.Sunday)]
    public void BetweenWeeks(DayOfWeek weekStart)
    {
        _creditLimitsSettings.UseRulesForPrimeId = false;
        _creditLimitsSettings.UseSuspuciousDaysForDouble = false;
        _creditLimitsSettings.FirstDayOfWeek = weekStart;

        var customerId = Guid.NewGuid().ToString();

        var today = DateTime.Today;
        var daysUntilNextWeekStart = ((int)weekStart - (int)today.DayOfWeek + 7) % 7;
        var nextWeekStart = today.AddDays(daysUntilNextWeekStart);

        CreateTransaction(customerId,
            _creditLimitsSettings.WeeklyLimitInBucks,
            nextWeekStart - TimeSpan.FromDays(1));

        _transactionsValidator.Validate(new Transaction
        {
            Id = "3",
            CustomerId = customerId,
            Amount = new LoadAmount(_creditLimitsSettings.DailyLimitInBucks, Currency.USD),
            Timestamp = nextWeekStart
        }).Should().BeTrue();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }

    private void CreateTransaction(string customerId, decimal amount, DateTimeOffset timestamp)
    {
        var transaction = new Transaction
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = customerId,
            Amount = new LoadAmount(amount, Currency.USD),
            Timestamp = timestamp
        };
        _transactionsRepository.PutAcceptedTransaction(transaction);
    }
}