namespace NGBills.DTOs
{
    public class WalletDtos
    {
        public class WalletDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public decimal Balance { get; set; }
            public string Currency { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public class FundWalletDto
        {
            public decimal Amount { get; set; }
            public string Email { get; set; } = string.Empty;
        }

        public class PaystackResponseDto
        {
            public decimal Amount { get; set; }
            public bool Status { get; set; }
            public string Message { get; set; } = string.Empty;
            public PaystackDataDto Data { get; set; } = new PaystackDataDto();
        }

        public class PaystackDataDto
        {
            public string AuthorizationUrl { get; set; } = string.Empty;
            public string AccessCode { get; set; } = string.Empty;
            public string Reference { get; set; } = string.Empty;
        }

        public class VerifyTransactionDto
        {
            public string Reference { get; set; } = string.Empty;
        }
    }
}
