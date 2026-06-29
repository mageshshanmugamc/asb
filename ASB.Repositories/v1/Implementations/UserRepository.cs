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

        public async Task<Models.PagedResult<User>> GetAllUsersAsync(Models.PaginationQuery pagination)
        {
            var query = _context.Users
                .Include(u => u.UserGroupMappings)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.NameFilter))
            {
                query = query.Where(u => u.Username.Contains(pagination.NameFilter));
            }

            query = pagination.SortBy?.ToLowerInvariant() switch
            {
                "email" => pagination.IsDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                "id" => pagination.IsDescending ? query.OrderByDescending(u => u.Id) : query.OrderBy(u => u.Id),
                _ => pagination.IsDescending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip(pagination.Skip).Take(pagination.Take).ToListAsync();

            return new Models.PagedResult<User> { Items = items, TotalCount = totalCount };
        }

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

        public async Task<IEnumerable<User>> GetUserById(int userId) => await _context.UserGroupMappings
            .Where(u => u.User.Id == userId)
            .Select(ugm => ugm.User)
            .Include(u => u.UserGroupMappings)
            .ToListAsync();
    }
}