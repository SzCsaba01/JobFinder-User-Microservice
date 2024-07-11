using User.Data.Access.Helpers.DTO.Authentication;

namespace User.Services.Contracts;
public interface IAuthenticationService
{
    public Task<AuthenticationResponseDto> LoginAsync(AuthenticationRequestDto request);
}
