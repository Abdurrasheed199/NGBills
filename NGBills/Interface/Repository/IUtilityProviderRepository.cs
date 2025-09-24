using NGBills.Entities;
using NGBills.Enum;

namespace NGBills.Interface.Repository
{
    public interface IUtilityProviderRepository : IRepository<UtilityProvider>
    {

        
        Task<UtilityProvider> GetProviderByNameAsync(string name);
        Task<IEnumerable<UtilityProvider>> GetActiveProvidersAsync();
        Task<IEnumerable<UtilityProvider>> GetProvidersByTypeAsync(BillType billType);
        Task<UtilityProvider> CreateProviderAsync(UtilityProvider provider);
        Task UpdateProviderAsync(UtilityProvider provider);
        Task<bool> DeactivateProviderAsync(int id);
        
    }
}
