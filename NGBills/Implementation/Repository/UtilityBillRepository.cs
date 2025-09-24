using Microsoft.EntityFrameworkCore;
using NGBills.Context;
using NGBills.Entities;
using NGBills.Interface.Repository;

namespace NGBills.Implementation.Repository
{
    public class UtilityBillRepository : Repository<UtilityBill>, IUtilityBillRepository
    {
        public UtilityBillRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<UtilityBill>> GetByUserIdAsync(int userId)
        {
            return await _context.UtilityBills
                .Include(b => b.User)
                .Include(b => b.Transaction)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.DueDate)
                .ToListAsync();
        }

        public async Task AddAsync(UtilityBill bill)
        {
            await _context.UtilityBills.AddAsync(bill);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UtilityBill bill)
        {
            _context.UtilityBills.Update(bill);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bill = await _context.UtilityBills.FindAsync(id);
            if (bill == null)
                return false;

            _context.UtilityBills.Remove(bill);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<UtilityBill> GetBillByReferenceAsync(string reference)
        {
            return await _context.UtilityBills
                .Include(b => b.UtilityProvider)
                .Include(b => b.Transaction)
                .FirstOrDefaultAsync(b => b.BillReference == reference || b.RetrievalReference == reference);
        }

        public async Task<IEnumerable<UtilityBill>> GetBillsByUserIdAsync(int userId)
        {
            return await _context.UtilityBills
                .Include(b => b.UtilityProvider)
                .Include(b => b.Transaction)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UtilityBill>> GetBillsByProviderAsync(int providerId)
        {
            return await _context.UtilityBills
                .Include(b => b.UtilityProvider)
                .Where(b => b.ProviderId == providerId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UtilityBill>> GetUnpaidBillsByUserIdAsync(int userId)
        {
            return await _context.UtilityBills
                .Include(b => b.UtilityProvider)
                .Where(b => b.UserId == userId && !b.IsPaid && b.DueDate >= DateTime.UtcNow.Date)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<UtilityBill?> GetUnpaidBillAsync(int userId, string accountNumber, int providerId)
        {
            return await _context.UtilityBills
                .Include(b => b.UtilityProvider)
                .FirstOrDefaultAsync(b =>
                        b.UserId == userId &&
                        b.AccountNumber == accountNumber &&
                        b.ProviderId == providerId &&
                        !b.IsPaid &&
                        b.DueDate >= DateTime.UtcNow.Date);
        }

        public async Task<IEnumerable<UtilityBill>> GetOverdueBillsAsync()
        {
            return await _context.UtilityBills
                .Include(b => b.UtilityProvider)
                .Where(b => !b.IsPaid && b.DueDate < DateTime.UtcNow.Date)
                .OrderBy(b => b.DueDate)
                .ToListAsync();
        }

        public async Task<UtilityBill> CreateBillAsync(UtilityBill bill)
        {
            bill.CreatedAt = DateTime.UtcNow;

            // Generate retrieval reference if not provided
            if (string.IsNullOrEmpty(bill.RetrievalReference))
            {
                bill.RetrievalReference = GenerateRetrievalReference();
            }

            await _context.UtilityBills.AddAsync(bill);
            await _context.SaveChangesAsync();
            return bill;
        }

        public async Task UpdateBillAsync(UtilityBill bill)
        {
            _context.UtilityBills.Update(bill);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> MarkBillAsPaidAsync(int billId, int transactionId)
        {
            var bill = await _context.UtilityBills.FindAsync(billId);
            if (bill == null) return false;

            bill.IsPaid = true;
            bill.PaidAt = DateTime.UtcNow;
            bill.TransactionId = transactionId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBillAsync(int id)
        {
            var bill = await _context.UtilityBills.FindAsync(id);
            if (bill == null) return false;

            _context.UtilityBills.Remove(bill);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        private string GenerateRetrievalReference()
        {
            return $"BILLRET_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }
    }
}
