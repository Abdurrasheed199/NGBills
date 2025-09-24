namespace NGBills.DTOs
{
    public class BillPaymentDtos
    {
        public class BillPaymentRequestDto
        {
            public required string BillType { get; set; } // "Electricity", "Water", etc.
            public required string MeterNumber { get; set; }
            public decimal Amount { get; set; }
            public string? CustomerName { get; set; }
        }

        public class BillPaymentResponseDto
        {
            public int TransactionId { get; set; }
            public string Status { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string ReferenceNumber { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
        }

        public class BillValidationRequestDto
        {
            public required string BillType { get; set; }
            public required string MeterNumber { get; set; }
        }

        public class BillValidationResponseDto
        {
            public bool IsValid { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public decimal OutstandingBalance { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}
