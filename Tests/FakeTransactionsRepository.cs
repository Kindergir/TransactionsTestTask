using JFF.DB;
using JFF.DTO;

namespace Tests;

// looks like copy, byt will not be with a real DB
public class FakeTransactionsRepository : ITransactionsRepository
{
    private Dictionary<string, IList<Transaction>> _transactions = new();

    public void PutAcceptedTransaction(Transaction action)
    {
        if (!_transactions.ContainsKey(action.CustomerId))
            _transactions.Add(action.CustomerId, new List<Transaction>());
        _transactions[action.CustomerId].Add(action);
    }

    public (decimal Sum, ulong Count) GetSumAndCountByPeriod(string customerId, Period period)
    {
        if (!_transactions.TryGetValue(customerId, out var transaction))
            return (Sum: 0m, Count: 0);

        return transaction
            .Where(x => period.Contains(x.Timestamp))
            .Aggregate<Transaction, (decimal Sum, ulong Count)>(
                (Sum: 0m, Count: 0),
                (acc, t) => (acc.Sum + t.Amount.Value, acc.Count + 1));
    }
}