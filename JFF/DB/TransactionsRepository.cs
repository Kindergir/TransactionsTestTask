using JFF.DTO;

namespace JFF.DB;

// aka PostgresDB, will be Repository
internal class TransactionsRepository : ITransactionsRepository
{
    private Dictionary<string, IList<Transaction>> _transactions = new();

    public void PutAcceptedTransaction(Transaction action)
    {
        if (!_transactions.ContainsKey(action.CustomerId))
            _transactions.Add(action.CustomerId, new List<Transaction>());
        _transactions[action.CustomerId].Add(action);
    }

    // would be better to filter and sum on DB side (only by applied DB transactions), not in memory
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