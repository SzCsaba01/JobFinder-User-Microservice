using User.Data.Object.Entities;

namespace User.Data.Contracts;
public interface IUserRepository
{
    public Task<UserEntity?> GetUserWithProfileByIdAsync(Guid userProfileId);
    public Task<List<UserEntity>> GetUsersByUserProfileIdsAsync(List<Guid> userProfileIds);
    public Task<UserEntity?> GetUserByEmailAsync(string email);
    public Task<UserEntity?> GetUserByUsernameAsync(string username);
    public Task<UserEntity?> GetUserByUsernameOrEmailAndPasswordAsync(string usernameOrEmail, string password);
    public Task<UserEntity?> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
    public Task<UserEntity?> GetUserByRegistrationTokenAsync(string registrationToken);
    public Task<UserEntity?> GetUserByResetPasswordTokenAsync(string resetPasswordToken);
    public Task<ICollection<UserEntity>> GetFilteredUsersAsync(string username, string? country, string? state, string? city, string? education, string? experience);
    public Task AddUserAsync(UserEntity user);
    public Task UpdateUserAsync(UserEntity user);
    public Task DeleteUserAsync(UserEntity user);
}
