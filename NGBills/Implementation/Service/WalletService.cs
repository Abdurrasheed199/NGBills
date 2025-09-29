using NGBills.Entities;
using NGBills.Enum;
using NGBills.Interface.Repository;
using NGBills.Interface.Service;
using static NGBills.DTOs.TransactionDtos;
using static NGBills.DTOs.WalletDtos;

namespace NGBills.Implementation.Service
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITransactionService _transactionService;
        private readonly IUserRepository _userRepository;
        private readonly IPaystackService _paystackService;
        private readonly ILogger<WalletService> _logger;

        public WalletService(
            IWalletRepository walletRepository,
            ITransactionRepository transactionRepository,
            ITransactionService transactionService,
            IUserRepository userRepository,
            IPaystackService paystackService,
            ILogger<WalletService> logger)
        {
            _walletRepository = walletRepository;
            _transactionRepository = transactionRepository;
            _transactionService = transactionService;
            _userRepository = userRepository;
            _paystackService = paystackService;
            _logger = logger;
        }

        public async Task<Wallet> GetWalletByUserIdAsync(int userId)
        {
            var wallet = await _walletRepository.GetByUserIdAsync(userId);
            if (wallet == null)
            {
                throw new Exception("Wallet not found for user");
            }
            return wallet;
        }

        public async Task<WalletDto> GetWalletDtoByUserIdAsync(int userId)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            return MapToDto(wallet);
        }

        public async Task ProcessPaymentVerification(string reference, int userId)
        {
            try
            {
                // Find the pending transaction
                var transaction = await _transactionRepository.GetByReferenceAsync(reference);
                if (transaction == null)
                {
                    throw new Exception("Transaction not found");
                }

                // Verify the transaction belongs to the user's wallet
                var wallet = await GetWalletByUserIdAsync(userId);
                if (transaction.WalletId != wallet.Id)
                {
                    throw new Exception("Transaction does not belong to user");
                }

                // Update transaction status to completed
                transaction.Status = TransactionStatus.Completed;
                _transactionRepository.Update(transaction);

                // Update wallet balance
                wallet.Balance += transaction.Amount;
                wallet.UpdatedAt = DateTime.UtcNow;
                _walletRepository.Update(wallet);

                await _walletRepository.SaveChangesAsync();

                _logger.LogInformation($"Successfully processed payment verification for reference: {reference}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing payment verification for reference: {reference}");
                throw;
            }
        }

        public async Task<decimal> GetWalletBalanceAsync(int userId)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            return wallet.Balance;
        }

        


        public async Task<bool> VerifyPaymentAsync(string reference)
        {
            var transaction = await _transactionRepository.GetByReferenceAsync(reference);

            if (transaction == null)
                throw new Exception("Transaction not found");

            if (transaction.Status != TransactionStatus.Pending)
                return transaction.Status == TransactionStatus.Successful;

            var verification = await _paystackService.VerifyTransaction(reference);

            try
            {
                // Update transaction status
                transaction.Status = TransactionStatus.Successful;
                 _transactionRepository.Update(transaction);

                // Update wallet balance
                var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);
                wallet.Balance += transaction.Amount;
                _walletRepository.Update(wallet);

                return true;
            }
            catch(Exception ex)
            {
                transaction.Status = TransactionStatus.Failed;
               _transactionRepository.Update(transaction);
                return false;
            }
        }


        public async Task<IEnumerable<TransactionResponseDto>> GetWalletTransactionsAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var wallet = await _walletRepository.GetByUserIdAsync(userId);
            if (wallet == null)
                throw new Exception("Wallet not found");

            // If your repository doesn't support pagination, you might need to add this method
            var transactions = wallet.Transactions
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return transactions.Select(MapToTransactionResponseDto);
        }

        public async Task<InitiateResponse> FundWalletAsync(decimal amount, string email, int userId)
        {
            var fundwallet = new FundWalletDto
            {
                Amount = amount,
                Email = email,
            };

            var user = await _userRepository.GetByIdAsync(userId);

            if(user == null)
            {
                throw new Exception("User Not Found");
            }

            return await _paystackService.InitializePayment(fundwallet);
        }


        private WalletDto MapToDto(Wallet wallet)
        {
            return new WalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                Currency = wallet.Currency,
                CreatedAt = wallet.CreatedAt,
                UpdatedAt = wallet.UpdatedAt
            };
        }

        private TransactionResponseDto MapToTransactionResponseDto(Transactions transaction)
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

       
    }
}
