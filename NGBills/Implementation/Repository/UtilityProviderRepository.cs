using Microsoft.EntityFrameworkCore;
using NGBills.Context;
using NGBills.Entities;
using NGBills.Enum;
using NGBills.Interface.Repository;

namespace NGBills.Implementation.Repository
{
    public class UtilityProviderRepository : Repository<UtilityProvider>, IUtilityProviderRepository
    {
        public UtilityProviderRepository(AppDbContext context) : base(context){ }


        public async Task<UtilityProvider> GetProviderByNameAsync(string name)
        {
            return await _context.UtilityProviders
                .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());
        }

        public async Task<UtilityProvider> GetProviderByCodeAsync(string code)
        {
            return await _context.UtilityProviders
                .FirstOrDefaultAsync(p => p.Code.ToLower() == code.ToLower());
        }

        public async Task<IEnumerable<UtilityProvider>> GetActiveProvidersAsync()
        {
            return await _context.UtilityProviders
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<UtilityProvider>> GetProvidersByTypeAsync(BillType billType)
        {
            return await _context.UtilityProviders
                .Where(p => p.Type == billType && p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<UtilityProvider> CreateProviderAsync(UtilityProvider provider)
        {
            provider.CreatedAt = DateTime.UtcNow;
            provider.UpdatedAt = DateTime.UtcNow;

            await _context.UtilityProviders.AddAsync(provider);
            await _context.SaveChangesAsync();
            return provider;
        }

        public async Task UpdateProviderAsync(UtilityProvider provider)
        {
            provider.UpdatedAt = DateTime.UtcNow;
            _context.UtilityProviders.Update(provider);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeactivateProviderAsync(int id)
        {
            var provider = await _context.UtilityProviders.FindAsync(id);
            if (provider == null) return false;

            provider.IsActive = false;
            provider.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

       
    }
}
