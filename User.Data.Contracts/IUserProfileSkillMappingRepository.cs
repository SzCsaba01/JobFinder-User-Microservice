using User.Data.Object.Entities;

namespace User.Data.Contracts;
public interface IUserProfileSkillMappingRepository
{
    public Task AddUserProfileSkillMappingsAsync(ICollection<UserProfileSkillMapping> userProfileSkillMappings);
    public Task DeleteUserProfileSkillMappingsAsync(ICollection<UserProfileSkillMapping> userProfileSkillMappings);
}
