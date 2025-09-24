using System.ComponentModel.DataAnnotations;

namespace NGBills.DTOs
{
    public class UtilityBillDtos
    {
        public class AddBillDto
        {
            public string BillType { get; set; }
            public string Provider { get; set; }
            public int UserId { get; set; }
            public decimal Amount { get; set; }
            public DateTime DueDate { get; set; }
        }


        public class PayBillDto
        {
            public int BillId { get; set; }
            public int UserId { get;set; }
        }

        public class UtilityBillResponseDto
        {
            public int Id { get; set; }
            public string BillType { get; set; }
            public string Provider { get; set; }
            public int UserId { get; set; }
            public decimal Amount { get; set; }
            public DateTime DueDate { get; set; }
            public bool IsPaid { get; set; }
            public DateTime? PaidDate { get; set; }
        }


        public class RetrieveBillDto
        {
            public int UserId { get; set; }
            public string AccountNumber { get; set; } 
            public int ProviderId { get; set; }       
        }

    }
}
