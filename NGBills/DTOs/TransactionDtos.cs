namespace NGBills.DTOs
{
    public class TransactionDtos
    {
        public class TransactionDto
        {
            public int Id { get; set; }
            public int WalletId { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Description { get; set; } = string.Empty;
            public string Reference { get; set; } = string.Empty;
            public string? BillType { get; set; }
            public string? MeterNumber { get; set; }
            public string? CustomerName { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class TransactionQueryDto
        {
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public string? Type { get; set; }
            public string? Status { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }

        public class TransactionResponseDto
        {
            public int Id { get; set; }
            public decimal Amount { get; set; }
            public string Type { get; set; }
            public string Status { get; set; }
            public string Reference { get; set; }
            public string Description { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
