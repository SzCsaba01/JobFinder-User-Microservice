using Microsoft.EntityFrameworkCore;
using User.Data.Access.Data;
using User.Data.Contracts;
using User.Data.Object.Entities;

namespace User.Data.Access;
public class SkillRepository : ISkillRepository
{
    private readonly DataContext _dataContext;

    public SkillRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<ICollection<SkillEntity>> GetAllUnmappedSkills()
    {
        return await _dataContext.Skills
            .Where(x => !x.UserProfiles.Any())
            .Include(x => x.UserProfiles)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ICollection<SkillEntity>> GetAllSkillsAsync()
    {
        return await _dataContext.Skills
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddSkillsAsync(List<SkillEntity> skills)
    {
        await _dataContext.Skills.AddRangeAsync(skills);
        await _dataContext.SaveChangesAsync();
    }

    public async Task DeleteSkillsAsync(List<SkillEntity> skills)
    {
        _dataContext.Skills.RemoveRange(skills);
        await _dataContext.SaveChangesAsync();
    }
}
