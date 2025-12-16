using System.Text.Json;
using JFF;
using JFF.DB;
using JFF.DTO;
using JFF.Logic;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<ITransactionsRepository, TransactionsRepository>();
services.AddSingleton<TransactionsValidator>();
services.AddSingleton<ICreditLimitsSettings, CreditLimitsSettings>();

await using var provider = services.BuildServiceProvider();

Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), AppSettings.InputFileName));
if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), AppSettings.InputFileName)))
{
    Console.WriteLine($"Sad but true: there is no input file");
    return;
}

var transactionsCache = provider.GetService(typeof(ITransactionsRepository)) as TransactionsRepository;
var transactionsValidator = provider.GetService(typeof(TransactionsValidator)) as TransactionsValidator;

if (transactionsCache == null || transactionsValidator == null)
{
    Console.WriteLine("Something went wrong with DI");
    return;
}

var jsonSerializerOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
};

await using (File.Create(AppSettings.OutputFileName));
await using var writer = new StreamWriter(AppSettings.OutputFileName);

// assumption: can read line by line
using var reader = new StreamReader(AppSettings.InputFileName);
while (await reader.ReadLineAsync() is { } line)
{
    try
    {
        // assumption: if we get an exception here, we just log it, but not put a line into an output file
        var transaction = JsonSerializer.Deserialize<Transaction>(line, jsonSerializerOptions)!;

        var isValid = transactionsValidator.Validate(transaction);
        if (isValid)
        {
            transactionsCache.PutAcceptedTransaction(transaction);
        }

        writer.WriteLine(JsonSerializer.Serialize(
            new TransactionValidationOutput(transaction.Id, transaction.CustomerId, isValid),
            jsonSerializerOptions));
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}