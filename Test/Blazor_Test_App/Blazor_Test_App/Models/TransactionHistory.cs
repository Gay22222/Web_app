namespace Blazor_Test_App.Models
{
    public class TransactionHistory
    {
        public int TransactionID { get; set; }
        public int OrderID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionStatus { get; set; }

        public Order Order { get; set; }
    }
}
