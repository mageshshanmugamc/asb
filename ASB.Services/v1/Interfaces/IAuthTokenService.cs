using ASB.Services.v1.Dtos;

namespace ASB.Services.v1.Interfaces;

public interface IAuthTokenService
{
    Task<AppTokenDto> GenerateAppTokenAsync(GenerateTokenDto dto);
}
