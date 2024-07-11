using User.Data.Access.Data;
using User.Data.Contracts;
using User.Data.Object.Entities;

namespace User.Data.Access;
public class UserProfileSkillMappingRepository : IUserProfileSkillMappingRepository
{
    private readonly DataContext _dataContext;

    public UserProfileSkillMappingRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task AddUserProfileSkillMappingsAsync(ICollection<UserProfileSkillMapping> userProfileSkillMappings)
    {
        await _dataContext.UserProfileSkillMappings.AddRangeAsync(userProfileSkillMappings);
        await _dataContext.SaveChangesAsync();
    }

    public async Task DeleteUserProfileSkillMappingsAsync(ICollection<UserProfileSkillMapping> userProfileSkillMappings)
    {
        _dataContext.UserProfileSkillMappings.RemoveRange(userProfileSkillMappings);
        await _dataContext.SaveChangesAsync();
    }
}
