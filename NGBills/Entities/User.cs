using System.ComponentModel.DataAnnotations;

namespace NGBills.Entities
{
    public class User
    {
        
        public int Id { get; set; } 
        public string Email { get; set; } 
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public  Wallet Wallet { get; set; }
        public virtual ICollection<UtilityBill> UtilityBills { get; set; } = new List<UtilityBill>();
    }
}
