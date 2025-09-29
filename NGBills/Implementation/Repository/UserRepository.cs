using Microsoft.EntityFrameworkCore;
using NGBills.Context;
using NGBills.Entities;
using NGBills.Interface.Repository;

namespace NGBills.Implementation.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }



        //public async Task<User> GetByIdAsync(int id)
        //{
        //    // ✅ Use await properly and don't use the same context instance concurrently
        //    return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        //}

        public async Task<User> GetByEmailAsync(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<bool> UserExistsAsync(string email) =>
            await _context.Users.AnyAsync(u => u.Email == email);
    }
}
