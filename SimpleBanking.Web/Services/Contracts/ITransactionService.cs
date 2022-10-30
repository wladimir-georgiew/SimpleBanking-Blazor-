using SimpleBanking.Web.Data.Models;
using SimpleBanking.Web.Models;

namespace SimpleBanking.Web.Services.Contracts
{
    public interface ITransactionService
    {
        TransactionResult Transfer(ApplicationUser debtor, ApplicationUser creditor, decimal amount);
        ICollection<TransactionHistory> GetTransactionHistory(ApplicationUser user);
    }
}
