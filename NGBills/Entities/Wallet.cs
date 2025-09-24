using System.ComponentModel.DataAnnotations;

namespace NGBills.Entities
{
    public class Wallet
    {
        
        public int Id { get; set; }
        public int UserId { get; set; }
        public  User User { get; set; } 
        public decimal Balance { get; set; } = 0;
        public string Currency { get; set; } = "NGN";
        public DateTime CreatedAt { get; set; } 
        public DateTime UpdatedAt { get; set; } 
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
