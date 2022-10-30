namespace SimpleBanking.Web
{
    public static class ConstantMessages
    {
        public const string InsufficentFunds = "You don't have enough balance to perform this transfer.";
        public const string ReceiverNotFound = "Account not found. Please enter valid receiver.";
        public const string SuccessfulTransfer = "Transfer successful";
        public const string InvalidAmount = "The transfer amount should be more than 0.";
        public const string NoUsers = "No users in the database you can send funds to. Please create another account and try again.";
    }
}
