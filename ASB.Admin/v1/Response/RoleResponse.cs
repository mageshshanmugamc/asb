namespace ASB.Admin.v1.Response
{
    using ASB.Services.v1.Dtos;

    public class RoleResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public static RoleResponse FromDto(RoleDto dto)
        {
            return new RoleResponse
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }

        public static IEnumerable<RoleResponse> FromDtoList(IEnumerable<RoleDto> dtos)
        {
            return dtos.Select(FromDto);
        }
    }
}
