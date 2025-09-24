using NGBills.Entities;

namespace NGBills.Interface.Repository
{
    public interface IWalletRepository : IRepository<Wallet>
    {
        Task<Wallet> GetByUserIdAsync(int userId);
        Task<Wallet> GetByUserEmailAsync(string email);
        Task<Wallet> CreateWalletAsync(Wallet wallet);

    }
}
