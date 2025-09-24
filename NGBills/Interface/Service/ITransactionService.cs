using NGBills.Entities;
using static NGBills.DTOs.TransactionDtos;

namespace NGBills.Interface.Service
{
    public interface ITransactionService
    {
        Task<TransactionDto> GetTransactionByIdAsync(int id, int userId);
        Task<IEnumerable<TransactionDto>> GetUserTransactionsAsync(int userId, TransactionQueryDto query);
        Task<IEnumerable<TransactionDto>> GetWalletTransactionsAsync(int walletId, TransactionQueryDto query);
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
    }
}
