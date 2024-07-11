using Microsoft.EntityFrameworkCore;
using User.Data.Access.Data;
using User.Data.Contracts;
using User.Data.Object.Entities;

namespace User.Data.Access;
public class UserProfileRepository : IUserProfileRepository
{
    private readonly DataContext _dataContext;

    public UserProfileRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<UserProfileEntity?> GetUserProfileByIdAsync(Guid userProfileId)
    {
        return await _dataContext.UserProfiles
            .Include(x => x.User)
            .Include(x => x.Skills)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userProfileId);
    }

    public async Task AddUserProfileAsync(UserProfileEntity userProfile)
    {
        await _dataContext.UserProfiles.AddAsync(userProfile);
        await _dataContext.SaveChangesAsync();
    }

    public async Task UpdateUserProfileAsync(UserProfileEntity userProfile)
    {
        _dataContext.UserProfiles.Update(userProfile);
        await _dataContext.SaveChangesAsync();
    }
}
