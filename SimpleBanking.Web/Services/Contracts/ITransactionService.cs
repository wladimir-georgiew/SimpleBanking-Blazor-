using SimpleBanking.Web.Data.Models;
using SimpleBanking.Web.Models;

namespace SimpleBanking.Web.Services.Contracts
{
    public interface ITransactionService
    {
        TransactionResult Transfer(string debtorEmail, string creditorEmail, decimal amount);
        ICollection<TransactionHistory> GetTransactionHistory(string userEmail, int pageNumber, int pageCount);
        int GetTransactionsTotalPages(string userEmail, int pageCount);
    }
}
