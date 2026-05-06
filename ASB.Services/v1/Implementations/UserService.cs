namespace ASB.Services.v1.Implementations
{
    using System.Collections.Generic;
    using ASB.Repositories.v1.Entities;
    using ASB.Repositories.v1.Interfaces;
    using ASB.Services.v1.Dtos;
    using ASB.Services.v1.Interfaces;

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)  
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<UserDto>> GetUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    UserGroupIds = user.UserGroupMappings.Select(ugm => ugm.UserGroupId).ToList()
                });
            }
            return userDtos;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var existing = await _userRepository.GetUserByEmailAsync(dto.Email);
            if (existing is not null)
                throw new InvalidOperationException($"User with email '{dto.Email}' already exists.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = string.Empty
            };

            var created = await _userRepository.CreateUserAsync(user);

            foreach (var groupId in dto.UserGroupIds)
            {
                await _userRepository.AddUserToGroupAsync(created.Id, groupId);
            }

            return new UserDto
            {
                Id = created.Id,
                Username = created.Username,
                Email = created.Email,
                UserGroupIds = dto.UserGroupIds
            };
        }

        public async Task AddUserToGroupAsync(int userId, int userGroupId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId)
                ?? throw new KeyNotFoundException($"User with Id {userId} not found.");

            await _userRepository.AddUserToGroupAsync(userId, userGroupId);
        }

        public async Task<UserDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user is null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                UserGroupIds = user.UserGroupMappings.Select(ugm => ugm.UserGroupId).ToList()
            };
        }
    }
}