using SimpleBanking.Web.Data.Models;
using SimpleBanking.Web.Models;

namespace SimpleBanking.Web.Services.Contracts
{
    public interface ITransactionService
    {
        TransactionResult Transfer(string debtorEmail, string creditorEmail, decimal amount);
        ICollection<TransactionHistory> GetTransactionHistory(string userEmail, int pageNumber, int pageCount, DateTime? startDate, DateTime? endDate);
        int GetTransactionsTotalPages(string userEmail, int pageCount, DateTime? startDate, DateTime? endDate);
    }
}
