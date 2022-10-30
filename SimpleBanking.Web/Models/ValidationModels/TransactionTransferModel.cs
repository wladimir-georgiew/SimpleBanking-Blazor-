using System.ComponentModel.DataAnnotations;

namespace SimpleBanking.Web.Models.ValidationModels
{
    public class TransactionTransferModel
    {
        [Required(ErrorMessage = ConstantMessages.InvalidAmount)]
        [Range(0.01, double.MaxValue, ErrorMessage = ConstantMessages.InvalidAmount)]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = ConstantMessages.ReceiverNotFound)]
        public string DebtorEmail { get; set; }
    }
}
