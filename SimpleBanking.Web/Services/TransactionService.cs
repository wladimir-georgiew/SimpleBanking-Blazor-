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

        public TransactionResult Transfer(string debtorEmail, string creditorEmail, decimal amount)
        {
            var creditor = _context.Users.FirstOrDefault(x => x.Email == creditorEmail);
            var debtor = _context.Users.FirstOrDefault(x => x.Email == debtorEmail);

            if (creditor == null)
                return new TransactionResult { Success = false, Message = ConstantMessages.CreditorNotFound, NewBalance = 0.00M };
            if (debtor == null)
                return new TransactionResult { Success = false, Message = ConstantMessages.ReceiverNotFound, NewBalance = creditor.Balance };

            // Force update entries to ensure the balance is correct, considering executing multiple transfers at the same time from different clients OR receiving funds at the same time while sending
            _context.Entry(creditor).Reload();
            _context.Entry(debtor).Reload();

            if (creditor.Balance < amount)
                return new TransactionResult { Success = false, Message = ConstantMessages.InsufficentFunds, NewBalance = creditor.Balance };

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

                creditor.Balance -= amount;
                debtor.Balance += amount;

                _context.Transactions.Add(transactionEntity);
                _context.Update(creditor);
                _context.Update(debtor);
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

        public ICollection<TransactionHistory> GetTransactionHistory(string userEmail)
        {
            var transactions = new List<TransactionHistory>();

            var dbUser = _context.Users.Where(x => x.Email == userEmail);

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
