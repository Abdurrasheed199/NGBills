using NGBills.Entities;

namespace NGBills.Interface.Repository
{
    public interface IUtilityBillRepository : IRepository<UtilityBill>
    {
        Task<IEnumerable<UtilityBill>> GetByUserIdAsync(int userId);
        Task AddAsync(UtilityBill bill);
        Task<bool> DeleteAsync(int id);
        Task UpdateAsync(UtilityBill bill);

        Task<UtilityBill> GetBillByReferenceAsync(string reference);
        Task<IEnumerable<UtilityBill>> GetBillsByUserIdAsync(int userId);
        Task<IEnumerable<UtilityBill>> GetBillsByProviderAsync(int providerId);
        Task<IEnumerable<UtilityBill>> GetUnpaidBillsByUserIdAsync(int userId);
        Task<UtilityBill> GetUnpaidBillAsync(int userId, string accountNumber, int providerId);
        Task<IEnumerable<UtilityBill>> GetOverdueBillsAsync();
        Task<UtilityBill> CreateBillAsync(UtilityBill bill);
        Task UpdateBillAsync(UtilityBill bill);
        Task<bool> MarkBillAsPaidAsync(int billId, int transactionId);
        Task<bool> DeleteBillAsync(int id);
        Task<bool> SaveAsync();
    }
}
