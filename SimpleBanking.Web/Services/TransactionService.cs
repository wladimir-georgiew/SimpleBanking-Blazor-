using Microsoft.EntityFrameworkCore;
using SimpleBanking.Web.Data;
using SimpleBanking.Web.Data.Enums;
using SimpleBanking.Web.Data.Models;
using SimpleBanking.Web.Models;
using SimpleBanking.Web.Services.Contracts;

namespace SimpleBanking.Web.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public TransactionResult Transfer(ApplicationUser debtor, ApplicationUser creditor, decimal amount)
        {
            if (creditor.Balance < amount)
                return new TransactionResult { Success = false, Message = ConstantMessages.InsufficentFunds, NewBalance = creditor.Balance };
            if (!_context.Users.Contains(debtor))
                return new TransactionResult { Success = false, Message = ConstantMessages.ReceiverNotFound, NewBalance = creditor.Balance };

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var transactionEntity = new Transaction()
                {
                    Amount = amount,
                    DebtorId = debtor.Id,
                    CreditorId = creditor.Id,
                    Type = TransactionType.Transfer,
                    Date = DateTime.UtcNow,
                };

                _context.Transactions.Add(transactionEntity);
                creditor.Balance -= amount;
                debtor.Balance += amount;

                _context.SaveChanges();

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new TransactionResult { Success = false, Message = ex.Message, NewBalance = creditor.Balance };
            }

            return new TransactionResult { Success = true, Message = ConstantMessages.SuccessfulTransfer, NewBalance = creditor.Balance };
        }

        public ICollection<TransactionHistory> GetTransactionHistory(ApplicationUser user)
        {
            var transactions = new List<TransactionHistory>();

            var dbUser = _context.Users.Where(x => x == user);

            var incomingTransfer = dbUser
                .Include(x => x.TransfersIncoming)
                .FirstOrDefault()
                ?.TransfersIncoming;

            var outgoingTransfer = dbUser
               .Include(x => x.TransfersOutgoing)
               .FirstOrDefault()
               ?.TransfersOutgoing;

            foreach (var transfer in incomingTransfer!)
            {
                var model = GetTransactionHistoryFromTransaction(transfer);
                model.IsIncomingTransfer = true;

                transactions.Add(model);
            }
            foreach (var transfer in outgoingTransfer!)
            {
                var model = GetTransactionHistoryFromTransaction(transfer);
                model.IsIncomingTransfer = false;

                transactions.Add(model);
            }

            return transactions.OrderByDescending(x => (DateTime.Parse(x.Date))).ToArray();
        }

        private TransactionHistory GetTransactionHistoryFromTransaction(Transaction transaction)
        {
            return new TransactionHistory
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Date = transaction.Date.ToString("g"),
                Type = transaction.Type.ToString()
            };
        }
    }
}
