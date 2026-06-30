namespace ASB.Services.v1.Implementations
{
    using System.Collections.Generic;
    using ASB.Notifier.v1.Interfaces;
    using ASB.Notifier.v1.Models;
    using ASB.Repositories.v1.Entities;
    using ASB.Repositories.v1.Interfaces;
    using ASB.Repositories.v1.Models;
    using ASB.Services.v1.Dtos;
    using ASB.Services.v1.Interfaces;

    /// <summary>
    /// Implementation of the IUserService interface.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserNotificationService _userNotificationService;

        public UserService(IUserRepository userRepository, IUserNotificationService userNotificationService)
        {
            _userRepository = userRepository;
            _userNotificationService = userNotificationService;
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

        public async Task<PagedResult<UserDto>> GetUsersAsync(PaginationQuery query)
        {
            var repoQuery = new ASB.Repositories.v1.Models.PaginationQuery
            {
                Skip = query.Skip,
                Take = query.Take,
                SortBy = query.SortBy,
                IsDescending = query.IsDescending,
                NameFilter = query.NameFilter
            };
            var result = await _userRepository.GetAllUsersAsync(repoQuery);
            return new PagedResult<UserDto>
            {
                TotalCount = result.TotalCount,
                Items = result.Items.Select(user => new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    UserGroupIds = user.UserGroupMappings.Select(ugm => ugm.UserGroupId).ToList()
                })
            };
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

            var userDto = new UserDto
            {
                Id = created.Id,
                Username = created.Username,
                Email = created.Email,
                UserGroupIds = dto.UserGroupIds
            };

            await _userNotificationService.NotifyUserCreatedAsync(new UserCreatedNotification
            {
                UserId = created.Id,
                Username = created.Username,
                Email = created.Email
            });

            return userDto;
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

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id) ?? throw new KeyNotFoundException($"User with Id {id} not found.");

            // Assuming there's a method to delete the user in the repository
            await _userRepository.DeleteUserAsync(id);

            await _userNotificationService.NotifyUserDeletedAsync(new UserDeletedNotification
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email
                }) ;
        }
    }
}