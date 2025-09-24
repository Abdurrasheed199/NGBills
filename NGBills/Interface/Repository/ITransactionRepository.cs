using NGBills.Entities;

namespace NGBills.Interface.Repository
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByWalletIdAsync(int walletId);
        Task<Transaction> GetByReferenceAsync(string reference);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId);
    }
}
