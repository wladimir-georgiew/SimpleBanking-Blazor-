namespace SimpleBanking.Web.Models
{
    public class TransactionResult
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public decimal NewBalance { get; set; }
    }
}
