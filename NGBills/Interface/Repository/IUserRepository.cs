using NGBills.Entities;

namespace NGBills.Interface.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<bool> UserExistsAsync(string email);
    }
}
