using Microsoft.EntityFrameworkCore;
using NGBills.Context;
using NGBills.Entities;
using NGBills.Interface.Repository;

namespace NGBills.Implementation.Repository
{
    public class WalletRepository : Repository<Wallet>, IWalletRepository
    {
        public WalletRepository(AppDbContext context) : base(context) { }

        public async Task<Wallet> GetByUserIdAsync(int userId) =>
            await _context.Wallets.FirstOrDefaultAsync(w => w.UserId.Equals(userId));

        public async Task<Wallet> GetByUserEmailAsync(string email) =>
            await _context.Wallets
                .Include(w => w.User)
                .FirstOrDefaultAsync(w => w.User.Email == email);

        public async Task<Wallet> CreateWalletAsync(Wallet wallet)
        {
            
            if(wallet is null)
            {
                throw new ArgumentNullException(nameof(wallet));
            }
            await _context.Wallets.AddAsync(wallet);
            await _context.SaveChangesAsync();
            return wallet;
             
            
        }
    }
}
