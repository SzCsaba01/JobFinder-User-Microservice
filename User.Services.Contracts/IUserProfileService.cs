using User.Data.Access.Helpers.DTO.UserProfile;

namespace User.Services.Contracts;
public interface IUserProfileService
{
    public Task<UserProfileDto> GetUserProfileByIdAsync(Guid userProfileId);
    public Task EditUserProfileByIdAsync(Guid userProfileId, UserProfileDto newUserProfile, bool updateDataFromCV = false);
    public Task SendRecommandJobsMessageAsync(Guid userProfileId);
}
