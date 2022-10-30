namespace SimpleBanking.Web.Models
{
    public class TransactionHistory
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncomingTransfer { get; set; }
    }
}
