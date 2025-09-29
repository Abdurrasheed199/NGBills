using NGBills.Enum;
using System.ComponentModel.DataAnnotations;

namespace NGBills.Entities
{
    public class Transactions
    {
        
        public int Id { get; set; }
        public int WalletId { get; set; }
        public Wallet Wallet { get; set; } 
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string? BillType { get; set; } // e.g., "Electricity", "Water"
        public string? MeterNumber { get; set; } // For bill payments
        public string? CustomerName { get; set; } // For bill payments
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; } 
    }
}
