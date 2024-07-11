using User.Data.Object.Entities;

namespace User.Services.Contracts;
public interface ITokenService
{
    public Task<string> GetAuthentificationJwtAsync(UserEntity user);
    public Task<string> GenerateRandomTokenAsync();
}
