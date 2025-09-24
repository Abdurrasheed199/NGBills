using Microsoft.Extensions.Options;
using NGBills.Interface.Service;
using static NGBills.DTOs.WalletDtos;
using System.Text.Json;
using System.Text;

namespace NGBills.Implementation.Service
{
    public class PaystackService : IPaystackService
    {
        private readonly HttpClient _httpClient;
        private readonly PaystackSettings _paystackSettings;
        private readonly ILogger<PaystackService> _logger;

        public PaystackService(
            HttpClient httpClient,
            IOptions<PaystackSettings> paystackSettings,
            ILogger<PaystackService> logger)
        {
            _httpClient = httpClient;
            _paystackSettings = paystackSettings.Value;
            _logger = logger;

            // Set base address and authorization header
            _httpClient.BaseAddress = new Uri("https://api.paystack.co/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_paystackSettings.SecretKey}");
        }

        public async Task<PaystackResponseDto> InitializeTransaction(FundWalletDto fundWalletDto)
        {
            try
            {
                var requestData = new
                {
                    amount = fundWalletDto.Amount * 100, // Convert to kobo
                    email = fundWalletDto.Email,
                    reference = GenerateReference(),
                    callback_url = $"{_paystackSettings.CallbackUrl}?reference={{reference}}"
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("transaction/initialize", content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var paystackResponse = JsonSerializer.Deserialize<PaystackResponseDto>(responseContent);

                return paystackResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Paystack transaction");
                throw;
            }
        }

        public async Task<bool> VerifyTransaction(string reference)
        {
            try
            {
                var response = await _httpClient.GetAsync($"transaction/verify/{reference}");
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var verificationResponse = JsonSerializer.Deserialize<PaystackVerificationResponse>(responseContent);

                return verificationResponse?.Data?.Status == "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Paystack transaction");
                return false;
            }
        }

        private string GenerateReference()
        {
            return $"WALLET_{DateTime.UtcNow:yyyyMMddHHmmss}_{new Random().Next(1000, 9999)}";
        }
    }

    public class PaystackVerificationResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public PaystackVerificationData Data { get; set; } = new PaystackVerificationData();
    }

    public class PaystackVerificationData
    {
        public string Status { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class PaystackSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
    }
}

