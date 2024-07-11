using User.Data.Access.Helpers.DTO.Authentication;
using User.Data.Contracts;
using User.Services.Contracts;
using Project.Services.Business.Exceptions;

namespace User.Services.Business;
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ITokenService _tokenService;

    public AuthenticationService
        (
            IUserRepository userRepository,
            IEncryptionService encryptionService,
            ITokenService tokenService
        )
    {
        _userRepository = userRepository;
        _encryptionService = encryptionService;
        _tokenService = tokenService;
    }

    public async Task<AuthenticationResponseDto> LoginAsync(AuthenticationRequestDto request)
    {
        request.Password = _encryptionService.GenerateHashedPassword(request.Password);

        var user = await _userRepository.GetUserByUsernameOrEmailAndPasswordAsync(request.UserCredential, request.Password);

        if (user == null)
        {
            throw new AuthenticationException("Invalid credentials!");
        }

        if (!user.IsEmailConfirmed)
        {
            throw new AuthenticationException("Email is not verified!");
        }

        var token = await _tokenService.GetAuthentificationJwtAsync(user);
        
        return new AuthenticationResponseDto
        {
            Token = token,
            Role = user.Role.RoleName
        };
    }
}
