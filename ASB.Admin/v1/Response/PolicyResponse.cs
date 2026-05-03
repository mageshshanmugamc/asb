namespace ASB.Admin.v1.Response
{
    using ASB.Services.v1.Dtos;

    public class PolicyResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Resource { get; set; }
        public required string Action { get; set; }

        public static PolicyResponse FromDto(PolicyDto dto)
        {
            return new PolicyResponse
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                Resource = dto.Resource,
                Action = dto.Action
            };
        }

        public static IEnumerable<PolicyResponse> FromDtoList(IEnumerable<PolicyDto> dtos)
        {
            return dtos.Select(FromDto);
        }
    }
}
