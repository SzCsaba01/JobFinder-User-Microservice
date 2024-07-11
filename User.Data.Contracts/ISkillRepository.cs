using User.Data.Object.Entities;

namespace User.Data.Contracts;
public interface ISkillRepository
{
    public Task<ICollection<SkillEntity>> GetAllUnmappedSkills();
    public Task<ICollection<SkillEntity>> GetAllSkillsAsync();
    public Task AddSkillsAsync(List<SkillEntity> skills);
    public Task DeleteSkillsAsync(List<SkillEntity> skills);
}
