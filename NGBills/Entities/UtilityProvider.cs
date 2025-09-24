using NGBills.Enum;

namespace NGBills.Entities
{
    public class UtilityProvider
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; } // Unique identifier (e.g., "IKEDC", "LWC")
        public BillType Type { get; set; }
        public string Description { get; set; }
        public string ApiEndpoint { get; set; } // For real integration
        public bool RequiresAccountValidation { get; set; }
        public string AccountNumberFormat { get; set; } // Regex pattern for validation
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<UtilityBill> Bills { get; set; }
    }
}
