namespace ASB.Services.v1.Interfaces
{
    using System.Collections.Generic;
    using ASB.Services.v1.Dtos;

    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetUsers();
        Task<UserDto> CreateUserAsync(CreateUserDto dto);
        Task AddUserToGroupAsync(int userId, int userGroupId);
    }
}