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
            if (amount <= 0)
                return new TransactionResult { Success = false, Message = ConstantMessages.InvalidAmount, NewBalance = creditor.Balance };

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

        public ICollection<TransactionHistory> GetTransactionHistory(string userEmail, int pageNumber, int pageCount, DateTime? startDate, DateTime? endDate)
        {
            var user = _context.Users.First(x => x.Email == userEmail);

            var transactions = GetTransactionQuery(user.Id, startDate, endDate)
                .OrderByDescending(x => x.Date)
                .Skip(pageCount * (pageNumber - 1))
                .Take(pageCount)
                .Select(transaction => new TransactionHistory
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    Date = transaction.Date.ToString("g"),
                    Type = transaction.Type.ToString(),
                    IsIncomingTransfer = transaction.DebtorId == user.Id ? true : false,
                    OppositeUserEmail = transaction.DebtorId == user.Id
                                            ? (transaction.Creditor != null ? transaction.Creditor.Email : "")
                                            : transaction.Debtor.Email,
                })
                .ToArray();

            return transactions;
        }

        public int GetTransactionsTotalPages(string userEmail, int pageCount, DateTime? startDate, DateTime? endDate)
        {
            var user = _context.Users.First(x => x.Email == userEmail);
            var transactionsCount = GetTransactionQuery(user.Id, startDate, endDate).Count();

            return (int)Math.Ceiling((double)transactionsCount / pageCount);
        }

        private IQueryable<Transaction> GetTransactionQuery(string userId, DateTime? startDate, DateTime? endDate)
        {
            return _context.Transactions
                    .Where(x => (x.DebtorId == userId || x.CreditorId == userId) &&
                                               x.Date > startDate &&
                                               x.Date < endDate!.Value.AddDays(1));
        }
    }
}
