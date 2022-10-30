using SimpleBanking.Web.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimpleBanking.Web.Data.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        public string DebtorId { get; set; }
        public string? CreditorId { get; set; }

        public virtual ApplicationUser Debtor { get; set; }
        public virtual ApplicationUser? Creditor { get; set; }
    }
}
