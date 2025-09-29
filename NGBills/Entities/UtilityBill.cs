using NGBills.Enum;

namespace NGBills.Entities
{
    public class UtilityBill
    {
        public int Id { get; set; }
        public string BillType { get; set; } 
        public string Provider { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        public int? TransactionId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Transactions Transaction { get; set; }
        public int ProviderId { get; set; }
        public BillType Type { get; set; }
        public string AccountNumber { get; set; }
        public decimal? PreviousBalance { get; set; }
        public decimal? CurrentCharge { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public string BillReference { get; set; }
        public string RetrievalReference { get; set; } 
        public virtual UtilityProvider UtilityProvider { get; set; }
        
    }


    public class BillRetrievalResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? BillId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class ProviderBillData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public string ReferenceNumber { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

}
