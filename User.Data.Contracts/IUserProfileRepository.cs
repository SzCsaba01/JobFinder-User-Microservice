using User.Data.Object.Entities;

namespace User.Data.Contracts;
public interface IUserProfileRepository
{
    public Task<UserProfileEntity?> GetUserProfileByIdAsync(Guid userProfileId);
    public Task AddUserProfileAsync(UserProfileEntity userProfile);
    public Task UpdateUserProfileAsync(UserProfileEntity userProfile);
}
