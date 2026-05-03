using ASB.Repositories.v1.Entities;

namespace ASB.Repositories.v1.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(User user);
        Task AddUserToGroupAsync(int userId, int userGroupId, string? assignedBy = null);
    }
}