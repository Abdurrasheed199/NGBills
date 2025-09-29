using NGBills.Entities;
using NGBills.Enum;
using NGBills.Interface.Repository;
using NGBills.Interface.Service;
using static NGBills.DTOs.TransactionDtos;

namespace NGBills.Implementation.Service
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IWalletRepository walletRepository,
            ILogger<TransactionService> logger)
        {
            _transactionRepository = transactionRepository;
            _walletRepository = walletRepository;
            _logger = logger;
        }

        public async Task<TransactionDto> GetTransactionByIdAsync(int id, int userId)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                return null;
            }

            // Verify the transaction belongs to the user
            var wallet = await _walletRepository.GetByIdAsync(transaction.WalletId);
            if (wallet == null || wallet.UserId != userId)
            {
                return null;
            }

            return MapToDto(transaction);
        }

        public async Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(int userId, TransactionQueryDto query)
        {
            var transactions = await _transactionRepository.GetByUserIdAsync(userId);
            var filteredTransactions = FilterTransactions(transactions, query);

            return filteredTransactions.Select(MapToDto);
        }

        public async Task<IEnumerable<TransactionDto>> GetWalletTransactionsAsync(int walletId, TransactionQueryDto query)
        {
            var transactions = await _transactionRepository.GetByWalletIdAsync(walletId);
            var filteredTransactions = FilterTransactions(transactions, query);

            return filteredTransactions.Select(MapToDto);
        }

        public async Task<Transactions> CreateTransactionAsync(Transactions transaction)
        {
            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChangesAsync();
            return transaction;
        }

        private IEnumerable<Transactions> FilterTransactions(IEnumerable<Transactions> transactions, TransactionQueryDto query)
        {
            var filtered = transactions.AsQueryable();

            // Filter by type
            if (!string.IsNullOrEmpty(query.Type))
            {
                filtered = filtered.Where(t => t.Type.ToString() == query.Type);
            }

            // Filter by status
            if (!string.IsNullOrEmpty(query.Status))
            {
                filtered = filtered.Where(t => t.Status.ToString() == query.Status);
            }

            // Filter by date range
            if (query.StartDate.HasValue)
            {
                filtered = filtered.Where(t => t.CreatedAt >= query.StartDate.Value);
            }

            if (query.EndDate.HasValue)
            {
                filtered = filtered.Where(t => t.CreatedAt <= query.EndDate.Value);
            }

            // Apply pagination
            return filtered
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();
        }

        private TransactionDto MapToDto(Transactions transaction)
        {
            return new TransactionDto
            {
                Id = transaction.Id,
                WalletId = transaction.WalletId,
                Type = transaction.Type.ToString(),
                Status = transaction.Status.ToString(),
                Amount = transaction.Amount,
                Description = transaction.Description,
                Reference = transaction.Reference,
                BillType = transaction.BillType,
                MeterNumber = transaction.MeterNumber,
                CustomerName = transaction.CustomerName,
                CreatedAt = transaction.CreatedAt
            };
        }
    }
}
