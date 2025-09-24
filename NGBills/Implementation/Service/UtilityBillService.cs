using NGBills.Entities;
using NGBills.Enum;
using NGBills.Interface.Repository;
using NGBills.Interface.Service;
using System.Threading;
using static NGBills.DTOs.TransactionDtos;
using static NGBills.DTOs.UtilityBillDtos;

namespace NGBills.Implementation.Service
{
    public class UtilityBillService : IUtilityBillService
    {
        private readonly IUtilityBillRepository _utilityBillRepository;
        private readonly IUtilityProviderRepository _providerRepository;
        private readonly IUserRepository _userRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;


        public UtilityBillService(
            IUtilityBillRepository utilityBillRepository,
            IUtilityProviderRepository providerRepository,
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            ITransactionRepository transactionRepository,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _utilityBillRepository = utilityBillRepository;
            _providerRepository = providerRepository;
            _userRepository = userRepository;
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _httpClient = httpClient;

         
        }


        public async Task<IEnumerable<UtilityBillResponseDto>> GetUserBillsAsync(int userId)
        {
            var bills = await _utilityBillRepository.GetByUserIdAsync(userId);
            return bills.Select(MapToUtilityBillResponseDto);
        }

        public async Task<UtilityBillResponseDto> GetBillByIdAsync(int userId, int billId)
        {
            var bill = await _utilityBillRepository.GetByIdAsync(billId);

            if (bill == null)
                throw new Exception("Bill not found");

            if (bill.UserId != userId)
                throw new Exception("Access denied");

            return MapToUtilityBillResponseDto(bill);
        }

        public async Task<UtilityBillResponseDto> AddBillAsync(int userId, AddBillDto addBillDto)
        {
            // Verify user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var bill = new UtilityBill
            {
                BillType = addBillDto.BillType,
                Provider = addBillDto.Provider,
                Amount = addBillDto.Amount,
                DueDate = addBillDto.DueDate,
                UserId = user.Id,
                IsPaid = false,
                PaidDate = null,
                TransactionId = null
            };

            await _utilityBillRepository.AddAsync(bill);

            return MapToUtilityBillResponseDto(bill);
        }

        public async Task<TransactionResponseDto> PayBillAsync(int userId, PayBillDto payBillDto)
        {
            var bill = await _utilityBillRepository.GetByIdAsync(payBillDto.BillId);

            if (bill == null)
                throw new Exception("Bill not found");

            if (bill.UserId != userId)
                throw new Exception("You can only pay your own bills");

            if (bill.IsPaid)
                throw new Exception("Bill has already been paid");

            var wallet = await _walletRepository.GetByUserIdAsync(userId);

            if (wallet.Balance < bill.Amount)
                throw new Exception("Insufficient balance");

            // Create transaction record
            var transaction = new Transaction
            {
                WalletId = wallet.Id,
                Amount = bill.Amount,
                Type = TransactionType.BillPayment,
                Status = TransactionStatus.Successful,
                Reference = $"bill_{Guid.NewGuid():N}",
                Description = $"Payment for {bill.BillType} bill - {bill.Provider}",
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transaction);

            // Update wallet balance
            wallet.Balance -= bill.Amount;
            _walletRepository.Update(wallet);

            // Update bill status
            bill.IsPaid = true;
            bill.PaidDate = DateTime.UtcNow;
            bill.TransactionId = transaction.Id;
            await _utilityBillRepository.UpdateAsync(bill);

            return MapToTransactionResponseDto(transaction);
        }

        public async Task<bool> DeleteBillAsync(int userId, int billId)
        {
            var bill = await _utilityBillRepository.GetByIdAsync(billId);

            if (bill == null)
                throw new Exception("Bill not found");

            if (bill.UserId != userId)
                throw new Exception("You can only delete your own bills");

            if (bill.IsPaid)
                throw new Exception("Cannot delete a paid bill");

            return await _utilityBillRepository.DeleteAsync(billId);
        }



        // ✅ BETTER: Retrieve bill from provider instead of creating manually
        public async Task<BillRetrievalResult> RetrieveBillFromProviderAsync(RetrieveBillDto retrieveDto)
        {
            // Validate provider exists and is active
            var provider = await _providerRepository.GetByIdAsync(retrieveDto.ProviderId);
            if (provider == null || !provider.IsActive)
            {
                return new BillRetrievalResult
                {
                    Success = false,
                    Message = "Utility provider not found or inactive"
                };
            }

            // ✅ Retrieve actual bill data from provider (mock implementation)
            var billData = await GetBillDataFromProvider(provider, retrieveDto.AccountNumber);

            if (!billData.Success)
            {
                return new BillRetrievalResult
                {
                    Success = false,
                    Message = billData.Message
                };
            }

            // Check if bill already exists and is unpaid
            var existingBill = await _utilityBillRepository.GetUnpaidBillAsync(
                retrieveDto.UserId,
                retrieveDto.AccountNumber,
                provider.Id
            );

            if (existingBill != null)
            {
                return new BillRetrievalResult
                {
                    Success = true,
                    Message = "Bill already exists",
                    BillId = existingBill.Id,
                    Amount = existingBill.Amount,
                    DueDate = existingBill.DueDate
                };
            }

            // Create new bill with VERIFIED data from provider
            var bill = new UtilityBill
            {
                UserId = retrieveDto.UserId,
                Type = provider.Type,
                Provider = provider.Name,
                ProviderId = provider.Id,
                AccountNumber = retrieveDto.AccountNumber,
                Amount = billData.Amount, // ✅ From provider, not user input
                DueDate = billData.DueDate, // ✅ From provider
                IsPaid = false,
                CreatedAt = DateTime.UtcNow,
                RetrievalReference = billData.ReferenceNumber
            };

            var createdBill = await _utilityBillRepository.CreateBillAsync(bill);

            return new BillRetrievalResult
            {
                Success = true,
                BillId = createdBill.Id,
                Amount = createdBill.Amount,
                DueDate = createdBill.DueDate,
                Message = "Bill retrieved successfully"
            };
        }

        public async Task<IEnumerable<UtilityProvider>> GetActiveProvidersAsync()
        {
            return await _providerRepository.GetActiveProvidersAsync();
        }


        // Manual mapping methods
        private UtilityBillResponseDto MapToUtilityBillResponseDto(UtilityBill bill)
        {
            if (bill == null) return null;

            return new UtilityBillResponseDto
            {
                Id = bill.Id,
                BillType = bill.BillType,
                Provider = bill.Provider,
                UserId = bill.UserId,
                Amount = bill.Amount,
                DueDate = bill.DueDate,
                IsPaid = bill.IsPaid,
                PaidDate = bill.PaidDate
            };
        }

        private TransactionResponseDto MapToTransactionResponseDto(Transaction transaction)
        {
            if (transaction == null) return null;

            return new TransactionResponseDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Type = transaction.Type.ToString(),
                Status = transaction.Status.ToString(),
                Reference = transaction.Reference,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt
            };
        }


      
        private async Task<ProviderBillData> GetBillDataFromProvider(UtilityProvider provider, string accountNumber)
        {
            try
            {
                // In real implementation, this would call the actual utility provider's API
                // For now, we'll mock the response based on provider type and account number

                // Simulate API call delay
                await Task.Delay(500);

                // Validate account number format (basic validation)
                if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length < 6)
                {
                    return new ProviderBillData { Success = false, Message = "Invalid account number" };
                }

                // Mock bill data based on provider type
                return provider.Type switch
                {
                    BillType.Electricity => new ProviderBillData
                    {
                        Success = true,
                        Amount = CalculateElectricityBill(accountNumber),
                        DueDate = DateTime.UtcNow.AddDays(15),
                        ReferenceNumber = $"ELEC-{DateTime.UtcNow:yyyyMMdd}-{accountNumber}"
                    },
                    BillType.Water => new ProviderBillData
                    {
                        Success = true,
                        Amount = CalculateWaterBill(accountNumber),
                        DueDate = DateTime.UtcNow.AddDays(10),
                        ReferenceNumber = $"WAT-{DateTime.UtcNow:yyyyMMdd}-{accountNumber}"
                    },
                    BillType.Internet => new ProviderBillData
                    {
                        Success = true,
                        Amount = CalculateInternetBill(accountNumber),
                        DueDate = DateTime.UtcNow.AddDays(20),
                        ReferenceNumber = $"INT-{DateTime.UtcNow:yyyyMMdd}-{accountNumber}"
                    },
                    _ => new ProviderBillData { Success = false, Message = "Unsupported bill type" }
                };
            }
            catch (Exception ex)
            {
                return new ProviderBillData { Success = false, Message = $"Provider error: {ex.Message}" };
            }
        }

        // Mock calculation methods (in real app, these would be actual provider API calls)
        private decimal CalculateElectricityBill(string accountNumber)
        {
            // Simple mock logic - in reality, this would be complex calculation from provider
            var baseAmount = 5000m; // Base amount
            var randomFactor = new Random().Next(80, 120) / 100m; // ±20% variation
            return Math.Round(baseAmount * randomFactor, 2);
        }

        private decimal CalculateWaterBill(string accountNumber) => 2500m;
        private decimal CalculateInternetBill(string accountNumber) => 15000m;

    }
}
