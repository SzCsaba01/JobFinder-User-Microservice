using User.Data.Object.Entities;

namespace User.Services.Contracts;
public interface IUserProfileDataProcessorService
{
    public Task UpdateUserProfileByCVDataAsync(UserProfileEntity userProfile);
    public Task AddNewSkillsAsync(List<string> skills);
    public List<string> GetUserSkillsToBeAdded(List<string> oldUserSkills, List<string> newUserSkills);
    public List<string> GetUserSkillsToBeRemoved(List<string> oldUserSkills, List<string> newUserSkills);
}
