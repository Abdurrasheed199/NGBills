using Microsoft.EntityFrameworkCore;
using NGBills.Context;
using NGBills.Interface.Repository;
using NGBills.Entities;

namespace NGBills.Implementation.Repository
{
    public class TransactionRepository : Repository<Transactions>, ITransactionRepository
    {
        public TransactionRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Transactions>> GetByWalletIdAsync(int walletId) =>
            await _context.Transactions
                .Where(t => t.WalletId.Equals(walletId))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<Transactions> GetByReferenceAsync(string reference) =>
            await _context.Transactions.FirstOrDefaultAsync(t => t.Reference == reference);

        public async Task<IEnumerable<Transactions>> GetByUserIdAsync(int userId) =>
            await _context.Transactions
                .Include(t => t.Wallet)
                .Where(t => t.Wallet.UserId.Equals(userId))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

    }
}
