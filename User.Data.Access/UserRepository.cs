using Microsoft.EntityFrameworkCore;
using User.Data.Access.Data;
using User.Data.Access.Helpers;
using User.Data.Contracts;
using User.Data.Object.Entities;

namespace User.Data.Access;
public class UserRepository : IUserRepository
{
    private readonly DataContext _dataContext;

    public UserRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<UserEntity?> GetUserWithProfileByIdAsync(Guid userProfileId)
    {
        return await _dataContext.Users
            .Where(u => u.UserProfile.Id == userProfileId)
            .Include(u => u.UserProfile)
                .ThenInclude(u => u.Skills)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<UserEntity?> GetUserByUsernameAsync(string username)
    {
        return await _dataContext.Users
            .Where(u => u.Username == username)
            .Include(u => u.UserProfile)
                .ThenInclude(up => up.Skills)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        return await _dataContext.Users
            .Where(u => u.Email == email)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<UserEntity?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _dataContext.Users
            .Where(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<UserEntity?> GetUserByUsernameOrEmailAndPasswordAsync(string usernameOrEmail, string password)
    {
        return await _dataContext.Users
            .Where(u => (u.Username == usernameOrEmail || u.Email == usernameOrEmail) && u.Password == password)
            .Include(u => u.Role)
            .Include(u => u.UserProfile)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<UserEntity?> GetUserByRegistrationTokenAsync(string registrationToken)
    {
        return await _dataContext.Users
            .Where(u => u.RegistrationToken == registrationToken)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<UserEntity?> GetUserByResetPasswordTokenAsync(string resetPasswordToken)
    {
        return await _dataContext.Users
            .Where(u => u.ResetPasswordToken == resetPasswordToken)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<UserEntity>> GetUsersPaginatedAsync(int page, int pageSize)
    {
        return await _dataContext.Users
            .Include(u => u.UserProfile)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<UserEntity>> GetUsersByUserProfileIdsAsync(List<Guid> userProfileIds)
    {
        return await _dataContext.Users
            .Where(u => userProfileIds.Contains(u.UserProfile.Id))
            .Include(u => u.UserProfile)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ICollection<UserEntity>> GetFilteredUsersAsync(string username, string? country, string? state, string? city, string? education, string? experience)
    {
        var usersQuery = _dataContext.Users
            .Where(u => u.Role.RoleName != AppConstants.ADMIN_ROLE)
            .Include(u => u.Role)
            .Include(u => u.UserProfile)
                .ThenInclude(up => up.Skills)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(username))
        {
            usersQuery = usersQuery.Where(u => u.Username.ToLower().Contains(username.ToLower()));
        }

        if (!string.IsNullOrEmpty(country))
        {
            usersQuery = usersQuery.Where(u => u.UserProfile.Country.ToLower().Contains(country.ToLower()));
        }

        if (!string.IsNullOrEmpty(state))
        {
            usersQuery = usersQuery.Where(u => u.UserProfile.State.ToLower().Contains(state.ToLower()));
        }

        if (!string.IsNullOrEmpty(city))
        {
            usersQuery = usersQuery.Where(u => u.UserProfile.City.ToLower().Contains(city.ToLower()));
        }

        if (!string.IsNullOrEmpty(education))
        {
            usersQuery = usersQuery.Where(u => u.UserProfile.Education.ToLower().Contains(education.ToLower()));
        }

        if (!string.IsNullOrEmpty(experience))
        {
            usersQuery = usersQuery.Where(u => u.UserProfile.Experience.ToLower().Contains(experience.ToLower()));
        }

        return await usersQuery.ToListAsync();
    }

    public async Task AddUserAsync(UserEntity user)
    {
        var userRole = await _dataContext.Roles
            .Where(r => r.RoleName == AppConstants.USER_ROLE)
            .FirstOrDefaultAsync();

        user.Role = userRole;

        await _dataContext.Users.AddAsync(user);
        await _dataContext.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(UserEntity user)
    {
        _dataContext.Users.Update(user);
        await _dataContext.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(UserEntity user)
    {
        _dataContext.Remove(user);
        await _dataContext.SaveChangesAsync();
    }
}
