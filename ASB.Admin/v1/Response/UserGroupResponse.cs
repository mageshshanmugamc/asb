namespace ASB.Admin.v1.Response
{
    using ASB.Services.v1.Dtos;

    public class UserGroupResponse
    {
        public int Id { get; set; }
        public required string GroupName { get; set; }
        public List<UserResponse> Users { get; set; } = [];
        public List<RoleResponse> Roles { get; set; } = [];

        public static UserGroupResponse FromDto(UserGroupDto dto)
        {
            return new UserGroupResponse
            {
                Id = dto.Id,
                GroupName = dto.GroupName,
                Users = dto.Users.Select(u => UserResponse.DtoToUsers(u)).ToList(),
                Roles = dto.Roles.Select(RoleResponse.FromDto).ToList()
            };
        }

        public static IEnumerable<UserGroupResponse> FromDtoList(IEnumerable<UserGroupDto> dtos)
        {
            return dtos.Select(FromDto);
        }
    }
}
