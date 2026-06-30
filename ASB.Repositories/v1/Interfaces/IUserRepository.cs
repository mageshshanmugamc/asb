using ASB.Repositories.v1.Entities;
using ASB.Repositories.v1.Models;

namespace ASB.Repositories.v1.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<PagedResult<User>> GetAllUsersAsync(PaginationQuery query);
        Task<User> CreateUserAsync(User user);
        Task AddUserToGroupAsync(int userId, int userGroupId, string? assignedBy = null);

        Task DeleteUserAsync(int id);
    }
}