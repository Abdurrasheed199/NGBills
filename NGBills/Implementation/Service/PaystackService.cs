using Microsoft.Extensions.Options;
using NGBills.Interface.Service;
using static NGBills.DTOs.WalletDtos;
using System.Text.Json;
using System.Text;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Transactions;
using static NGBills.DTOs.TransactionDtos;
using NGBills.Context;
using NGBills.Entities;
using System.Net;
using Paystack.Net.SDK;
using PayStack.Net;


namespace NGBills.Implementation.Service
{
    public class PaystackService : IPaystackService
    {
        private readonly HttpClient _httpClient;
        private readonly PaystackSettings _paystackSettings;
        private readonly IConfiguration _configuration;
        private Paystack.Net.SDK.IPayStackApi payStackApi;
        private readonly string secretKey;
        private readonly AppDbContext _context;
        private readonly ILogger<PaystackService> _logger;

        public PaystackService(
            HttpClient httpClient,
            IOptions<PaystackSettings> paystackSettings,
            IConfiguration configuration,
            AppDbContext context,
            ILogger<PaystackService> logger)
        {
            _httpClient = httpClient;
            _paystackSettings = paystackSettings.Value;
            _configuration = configuration;
            secretKey = _configuration["Paystack:SecretKey"];
            payStackApi = new Paystack.Net.SDK.PayStackApi(secretKey);
            _context = context;
            _logger = logger;

            // Set base address and authorization header
            //_httpClient.BaseAddress = new Uri("https://api.paystack.co/");
            //_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_paystackSettings.SecretKey}");

            // Configure HttpClient
            _httpClient.BaseAddress = new Uri("https://api.paystack.co/");
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _paystackSettings.SecretKey);
        }


        public async Task<InitiateResponse> InitializePayment(FundWalletDto fundWalletDto)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var referenceId = Guid.NewGuid().ToString();

            try
            {
                // Get the user's wallet first
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.User.Email == fundWalletDto.Email);

                if (wallet == null)
                {
                    return new InitiateResponse
                    {
                        Status = false,
                        Message = "Wallet not found for this user",
                        Reference = referenceId
                    };
                }

                var paystackRequest = new Paystack.Net.SDK.Models.TransactionInitializationRequestModel
                {
                    amount = (int)fundWalletDto.Amount * 100,
                    email = fundWalletDto.Email,
                    reference = referenceId,
                    callbackUrl = _configuration["Paystack:CallBackUrl"]
                };

                _logger.LogInformation("InitializePayment Request: {Request}",
                    JsonConvert.SerializeObject(paystackRequest));

                var responseFromPaystack = await payStackApi.Transactions.InitializeTransaction(paystackRequest);

                _logger.LogInformation("Response from Paystack: {Response}",
                    JsonConvert.SerializeObject(responseFromPaystack));

                if (responseFromPaystack.status && responseFromPaystack.data != null)
                {
                    var transaction = new Transactions
                    {
                        Amount = fundWalletDto.Amount,
                        Status = Enum.TransactionStatus.Pending,
                        Reference = referenceId,
                        WalletId = wallet.Id, 
                        Description = "Wallet funding",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.AddAsync(transaction);
                    await _context.SaveChangesAsync();

                    return new InitiateResponse
                    {
                        Status = responseFromPaystack.status,
                        Message = responseFromPaystack.message,
                        AuthorizationUrl = responseFromPaystack.data.authorization_url,
                        AccessCode = responseFromPaystack.data.access_code,
                        Reference = responseFromPaystack.data.reference,
                    };
                }
                else
                {
                    _logger.LogWarning("Paystack initialization failed. Status: {Status}, Message: {Message}, Data: {Data}",
                        responseFromPaystack.status, responseFromPaystack.message,
                        responseFromPaystack.data == null ? "null" : "has data");

                    return new InitiateResponse
                    {
                        Status = false,
                        Message = responseFromPaystack.message ?? "Payment initialization failed",
                        AuthorizationUrl = null,
                        AccessCode = null,
                        Reference = referenceId
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in InitializePayment");
                return new InitiateResponse
                {
                    Status = false,
                    Message = $"An error occurred initializing payment: {ex.Message}",
                    AuthorizationUrl = null,
                    AccessCode = null,
                    Reference = referenceId
                };
            }
        }








        public async Task<PaystackVerifyResponse> VerifyTransaction(string reference)
        {
            //try
            //{
            //    PaystackVerifyResponse response = new PaystackVerifyResponse();
            //    _logger.LogInformation($"Reference:: {reference}");
            //    TransactionVerifyResponse verifyResponse = payStackApi.Transactions.VerifyTransaction(reference);
            //    if (verifyResponse.Status)
            //    {
            //        _logger.LogInformation($"Response From Paystack:: {JsonConvert.SerializeObject(verifyResponse)}");

            //        var updateTransaction = new Transactions
            //        {
            //            Amount = verifyResponse.Data.Amount,
            //            Status = Enum.TransactionStatus.Successful

            //        };
            //        _context.Update(updateTransaction);
            //        await _context.SaveChangesAsync();
            //    }
            //    else
            //    {
            //        _logger.LogInformation($"Response From Paystack:: {JsonConvert.SerializeObject(verifyResponse)}");
            //        var updateTransaction = new Transactions
            //        {
            //            Status = Enum.TransactionStatus.Failed,
            //            Reference = verifyResponse.Data.Reference

            //        };
            //        _context.Update(updateTransaction);
            //        await _context.SaveChangesAsync();
            //    }
            //    response = new PaystackVerifyResponse
            //    {
            //        Status = verifyResponse.Status,
            //        Message = verifyResponse.Message,
            //    };
            //    return response;

            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError("An error occured while verifying payment", ex.Message);
            //    var response = new PaystackVerifyResponse
            //    {
            //        Status = false,
            //        Message = "Error occured while verifying payment"
            //    };
            //    return response;
            //}
            return null;
        }

        

       
    }

   
    

    

    public class PaystackSettings
    {
        public string SecretKey { get; set; }
        public string PublicKey { get; set; }
        public string CallbackUrl { get; set; }
    }
}

