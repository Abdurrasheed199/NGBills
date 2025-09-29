using NGBills.Entities;

namespace NGBills.Interface.Repository
{
    public interface ITransactionRepository : IRepository<Transactions>
    {
        Task<IEnumerable<Transactions>> GetByWalletIdAsync(int walletId);
        Task<Transactions> GetByReferenceAsync(string reference);
        Task<IEnumerable<Transactions>> GetByUserIdAsync(int userId);
    }
}
