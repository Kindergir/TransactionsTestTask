using JFF.DTO;

namespace JFF.DB;

public interface ITransactionsRepository
{
    void PutAcceptedTransaction(Transaction action);
    (decimal Sum, ulong Count) GetSumAndCountByPeriod(string customerId, Period period);
}