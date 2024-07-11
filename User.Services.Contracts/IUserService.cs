using User.Data.Access.Helpers.DTO.User;

namespace User.Services.Contracts;
public interface IUserService
{
    public Task<FilteredUsersPaginationDto> GetFilteredUsersPaginatedAsync(FilteredUsersSearchDto userSearchDto);
    public Task RegisterUserAsync(UserRegistrationDto user);
    public Task VerifyEmailByRegistrationTokenAsync(string registrationToken);
    public Task<bool> VerifyIfResetPasswordTokenExistsAsync(string resetPasswordToken);
    public Task SendResetPasswordTokenByEmailAsync(string email);
    public Task ChangePasswordAsync(UserChangePasswordDto changePasswordDto);
    public Task DeleteUserByUsernameAsync(string username);
}
