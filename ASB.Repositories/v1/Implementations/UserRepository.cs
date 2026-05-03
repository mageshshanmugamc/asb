namespace ASB.Repositories.v1.Implementations
{
    using ASB.Repositories.v1.Contexts;
    using ASB.Repositories.v1.Entities;
    using ASB.Repositories.v1.Interfaces;
    using Microsoft.EntityFrameworkCore;

    public class UserRepository : IUserRepository
    {
        private readonly AsbContext _context;
        public UserRepository(AsbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByIdAsync(int userId) => await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        public async Task<User?> GetUserByEmailAsync(string email) => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<IEnumerable<User>> GetAllUsersAsync() => await _context.Users
            .Include(u => u.UserGroupMappings)
            .ToListAsync();

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task AddUserToGroupAsync(int userId, int userGroupId, string? assignedBy = null)
        {
            var exists = await _context.UserGroupMappings
                .AnyAsync(ugm => ugm.UserId == userId && ugm.UserGroupId == userGroupId);

            if (exists)
                throw new InvalidOperationException($"User {userId} is already a member of group {userGroupId}.");

            _context.UserGroupMappings.Add(new UserGroupMapping
            {
                UserId = userId,
                UserGroupId = userGroupId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = assignedBy
            });
            await _context.SaveChangesAsync();
        }
    }
}